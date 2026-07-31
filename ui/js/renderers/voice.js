// Voice Bitmask Slot Cards & UI Config Updater
function initVoiceSlotCheckboxes() {
    const containerT = document.getElementById('containerTSlots');
    const containerCT = document.getElementById('containerCTSlots');
    if (!containerT || !containerCT) return;

    containerT.innerHTML = '';
    containerCT.innerHTML = '';

    if (!State.currentMatchData || !State.currentMatchData.players || State.currentMatchData.players.length === 0) {
        containerT.innerHTML = '<div style="color: var(--text-dim); font-size: 11px; text-align: center; padding: 16px;">Load a demo file to see real player slots</div>';
        containerCT.innerHTML = '<div style="color: var(--text-dim); font-size: 11px; text-align: center; padding: 16px;">Load a demo file to see real player slots</div>';
        return;
    }

    const players = State.currentMatchData.players;
    players.forEach((p) => {
        const slotIdx = p.slotIndex !== undefined ? p.slotIndex : players.indexOf(p);
        const isT = p.team === 'T';
        const isChecked = State.customSlots[slotIdx] !== false;
        const card = document.createElement('div');
        card.className = `slot-card ${isT ? 'team-t' : 'team-ct'}`;
        card.onclick = (e) => {
            if (e.target.tagName !== 'INPUT') {
                const chk = card.querySelector('input');
                chk.checked = !chk.checked;
                toggleSlot(slotIdx, chk.checked);
            }
        };

        card.innerHTML = `
            <div style="display: flex; align-items: center; gap: 8px;">
                <span class="${isT ? 'badge-t' : 'badge-ct'}">${isT ? '🟧 T' : '🟦 CT'} Slot ${slotIdx}</span>
                <div style="font-weight: 700; font-size: 12px; color: var(--text-main);">${escapeHtml(p.name)}</div>
            </div>
            <div style="display: flex; align-items: center; gap: 10px;">
                <div style="font-size: 10px; color: var(--text-dim);">${p.kills}K / ${p.deaths}D</div>
                <input type="checkbox" ${isChecked ? 'checked' : ''} onchange="toggleSlot(${slotIdx}, this.checked)" style="accent-color: ${isT ? 'var(--accent-orange)' : 'var(--accent-blue)'}; cursor: pointer; transform: scale(1.2);">
            </div>
        `;

        if (isT) containerT.appendChild(card);
        else containerCT.appendChild(card);
    });
}

function calculateBitmaskFromSlots() {
    let mask = 0n;
    const slotsObj = State.customSlots || {};
    const maxSlot = Object.keys(slotsObj).reduce((m, k) => Math.max(m, parseInt(k) || 0), 63);
    const limit = Math.min(maxSlot, 63);
    for (let i = 0; i <= limit; i++) {
        if (slotsObj[i] !== false) {
            mask |= (1n << BigInt(i));
        }
    }
    return mask.toString();
}

function calculateTeamBitmasks() {
    let tMask = 0n;
    let ctMask = 0n;
    if (State.currentMatchData && State.currentMatchData.players) {
        State.currentMatchData.players.forEach(p => {
            const slotIdx = p.slotIndex !== undefined ? p.slotIndex : 0;
            if (slotIdx >= 0 && slotIdx < 64) {
                if (p.team === 'T') tMask |= (1n << BigInt(slotIdx));
                else if (p.team === 'CT') ctMask |= (1n << BigInt(slotIdx));
            }
        });
    } else {
        tMask = 31n;
        ctMask = 992n;
    }
    return { tMask: tMask.toString(), ctMask: ctMask.toString() };
}

function toggleSlot(slotIdx, isChecked) {
    State.customSlots[slotIdx] = isChecked;
    State.currentMode = 'CUSTOM';
    updateVoiceConfigUI();
}

function setPresetMode(mode) {
    State.currentMode = mode;
    if (!State.currentMatchData || !State.currentMatchData.players) {
        updateVoiceConfigUI();
        return;
    }
    const players = State.currentMatchData.players;
    State.customSlots = {};
    players.forEach(p => {
        const slotIdx = p.slotIndex !== undefined ? p.slotIndex : players.indexOf(p);
        if (mode === 'T') State.customSlots[slotIdx] = p.team === 'T';
        else if (mode === 'CT') State.customSlots[slotIdx] = p.team === 'CT';
        else State.customSlots[slotIdx] = true;
    });
    initVoiceSlotCheckboxes();
    updateVoiceConfigUI();
}

function updateVoiceConfigUI() {
    document.querySelectorAll('#btnVoiceT, #btnVoiceCT, #btnVoiceALL').forEach(b => b.classList.remove('active'));
    if (State.currentMode === 'T') document.getElementById('btnVoiceT').classList.add('active');
    else if (State.currentMode === 'CT') document.getElementById('btnVoiceCT').classList.add('active');
    else if (State.currentMode === 'ALL') document.getElementById('btnVoiceALL').classList.add('active');

    const ALL_MASK_STR = "18446744073709551615";
    const maskDec = (State.currentMode === 'ALL') ? ALL_MASK_STR : calculateBitmaskFromSlots();
    const { tMask, ctMask } = calculateTeamBitmasks();
    const cmd = `sv_cheats 1; tv_listen_voice_indices ${maskDec}`;
    const liveCmd = document.getElementById('liveConsoleCmd');
    if (liveCmd) liveCmd.innerText = cmd;
    const payload = { mode: State.currentMode, customMask: maskDec, tMask, ctMask };
    if (window.DemoPulseBridge) {
        window.DemoPulseBridge.send("GENERATE_VOICE_CFG", payload).catch(console.error);
    } else {
        sendToCSharp(`GENERATE_VOICE_CFG:${State.currentMode}:${maskDec}:${tMask}:${ctMask}`);
    }
}

function copyConsoleCmd() {
    const liveCmd = document.getElementById('liveConsoleCmd');
    if (liveCmd) {
        navigator.clipboard.writeText(liveCmd.innerText).then(() => {
            alert("Console command copied to clipboard!\n" + liveCmd.innerText);
        }).catch(() => {
            alert("Console command: " + liveCmd.innerText);
        });
    }
}
