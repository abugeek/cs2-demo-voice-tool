// DemoPulse Application Bootstrapper & UI Orchestrator

function showLoadingState() {
    const dropZone = document.getElementById('dropZone');
    const loadingState = document.getElementById('loadingState');
    const dashboard = document.getElementById('dashboard');
    if (dropZone) dropZone.style.display = 'none';
    if (dashboard) dashboard.style.display = 'none';
    if (loadingState) loadingState.style.display = 'flex';
}

function showError(msg) {
    const dropZone = document.getElementById('dropZone');
    const loadingState = document.getElementById('loadingState');
    const dashboard = document.getElementById('dashboard');
    const toast = document.getElementById('errorToast');
    if (loadingState) loadingState.style.display = 'none';
    if (dashboard) dashboard.style.display = 'none';
    if (dropZone) dropZone.style.display = 'block';
    if (toast) { toast.style.display = 'block'; toast.innerText = '⚠️ ' + msg; }
}

function switchTab(btnElem, tabId) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));

    if (btnElem && btnElem.classList) {
        btnElem.classList.add('active');
    } else if (window.event && window.event.currentTarget) {
        window.event.currentTarget.classList.add('active');
    }
    const targetTab = document.getElementById('tab-' + tabId);
    if (targetTab) targetTab.classList.add('active');
}

function switchSubTab(subTabName) {
    State.currentSubTab = subTabName;
    document.querySelectorAll('.subtab-btn').forEach(btn => btn.classList.remove('active'));
    if (window.event && window.event.currentTarget) {
        window.event.currentTarget.classList.add('active');
    }
    renderScoreboardCategory();
}

function setGroupMode(mode) {
    State.currentGroupMode = mode;
    document.getElementById('btnGroupTeam').classList.toggle('active', mode === 'team');
    document.getElementById('btnGroupPlayer').classList.toggle('active', mode === 'player');
    renderScoreboardCategory();
}

function setTeamFilter(filter) {
    State.currentTeamFilter = filter;
    document.getElementById('btnFilterAll').classList.toggle('active', filter === 'ALL');
    document.getElementById('btnFilterT').classList.toggle('active', filter === 'T');
    document.getElementById('btnFilterCT').classList.toggle('active', filter === 'CT');
    renderScoreboardCategory();
}

function renderFullDemo(data) {
    State.currentMatchData = data;
    const dropZone = document.getElementById('dropZone');
    const dashboard = document.getElementById('dashboard');
    const loadingState = document.getElementById('loadingState');
    const errorToast = document.getElementById('errorToast');
    if (dropZone) dropZone.style.display = 'none';
    if (loadingState) loadingState.style.display = 'none';
    if (errorToast) errorToast.style.display = 'none';
    if (dashboard) dashboard.style.display = 'flex';
    State.customSlots = {};

    document.getElementById('matchFileName').innerText = data.meta.fileName;
    document.getElementById('matchMeta').innerText = `Map: ${data.meta.map} | Server: ${data.meta.server} | Score: ${data.meta.scoreCT} - ${data.meta.scoreT} (${data.meta.winner} Win)`;

    renderMvpHeroBanner(data.players);
    renderScoreboardCategory();
    renderDuelsMatrix(data.duels);
    renderClutchesTable(data.clutches);
    renderUtilityTable(data.utility);
    initVoiceSlotCheckboxes();
    updateVoiceConfigUI();
}

// Global Keyboard Shortcuts (B / N / V / C)
window.addEventListener('keydown', (e) => {
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT') return;
    const key = e.key.toUpperCase();

    if (key === 'B') {
        e.preventDefault();
        setPresetMode('T');
    } else if (key === 'N') {
        e.preventDefault();
        setPresetMode('CT');
    } else if (key === 'V') {
        e.preventDefault();
        setPresetMode('ALL');
    } else if (key === 'C') {
        e.preventDefault();
        copyConsoleCmd();
    }
});

function openRenameModal() {
    if (!State.currentMatchData || !State.currentMatchData.meta) return;
    const input = document.getElementById('renameInput');
    if (input) input.value = State.currentMatchData.meta.fileName || '';
    const modal = document.getElementById('renameModal');
    if (modal) modal.classList.add('active');
}

function closeRenameModal() {
    const modal = document.getElementById('renameModal');
    if (modal) modal.classList.remove('active');
}

function submitRenameDemo() {
    const input = document.getElementById('renameInput');
    if (!input) return;
    const newName = input.value.trim();
    if (!newName) {
        alert("Please enter a valid demo file name.");
        return;
    }

    window.renameCurrentDemo(newName).then(res => {
        closeRenameModal();
    }).catch(err => {
        alert("Failed to rename demo file: " + err.message);
    });
}

window.addEventListener('DOMContentLoaded', () => {
    initDragAndDrop();
    document.getElementById('dropZone').style.display = 'block';
    document.getElementById('dashboard').style.display = 'none';
    document.getElementById('loadingState').style.display = 'none';
    initVoiceSlotCheckboxes();
    requestSettings();
});
