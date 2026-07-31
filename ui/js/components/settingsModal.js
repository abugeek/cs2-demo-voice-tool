// DemoPulse Settings Modal Component
function openSettingsModal() {
    requestSettings();
    const modal = document.getElementById('settingsModal');
    if (modal) modal.classList.add('active');
}

function closeSettingsModal() {
    const modal = document.getElementById('settingsModal');
    if (modal) modal.classList.remove('active');
}

function updateCfgPreview(val) {
    const preview = document.getElementById('cfgNamePreview');
    if (preview) preview.innerText = val.trim() || 'demopulse';
}

function populateSettingsModal(s) {
    State.appSettings = s;
    const cfgInput = document.getElementById('cfgFileNameInput');
    const cfgPreview = document.getElementById('cfgNamePreview');
    const cs2Folder = document.getElementById('cs2FolderInput');
    const keyT = document.getElementById('keyTInput');
    const keyCt = document.getElementById('keyCtInput');
    const keyAll = document.getElementById('keyAllInput');
    const keyMute = document.getElementById('keyMuteInput');
    const keySpeedUp = document.getElementById('keySpeedUpInput');
    const keySlowMo = document.getElementById('keySlowMoInput');
    const keyPause = document.getElementById('keyPauseInput');
    const keyReset = document.getElementById('keyResetInput');
    const autoSave = document.getElementById('autoSaveCheckbox');
    const autoCopy = document.getElementById('autoCopyDemoCheckbox');

    if (cfgInput) cfgInput.value = s.ConfigFileName || 'demopulse';
    if (cfgPreview) cfgPreview.innerText = s.ConfigFileName || 'demopulse';
    if (cs2Folder) cs2Folder.value = s.Cs2CfgFolder || '';
    if (keyT) keyT.value = s.KeyBindT || 'b';
    if (keyCt) keyCt.value = s.KeyBindCT || 'n';
    if (keyAll) keyAll.value = s.KeyBindAll || 'v';
    if (keyMute) keyMute.value = s.KeyBindMute || 'm';
    if (keySpeedUp) keySpeedUp.value = s.KeyBindSpeedUp || 'shift';
    if (keySlowMo) keySlowMo.value = s.KeyBindSlowMo || 'ctrl';
    if (keyPause) keyPause.value = s.KeyBindPause || 'space';
    if (keyReset) keyReset.value = s.KeyBindResetSpeed || 'r';
    if (autoSave) autoSave.checked = s.AutoSaveToCs2 !== false;
    if (autoCopy) autoCopy.checked = s.AutoCopyDemoToCs2 !== false;
}

function saveSettingsFromModal() {
    State.appSettings.ConfigFileName = document.getElementById('cfgFileNameInput').value.trim() || 'demopulse';
    State.appSettings.Cs2CfgFolder = document.getElementById('cs2FolderInput').value.trim();
    State.appSettings.KeyBindT = document.getElementById('keyTInput').value.trim().toLowerCase() || 'b';
    State.appSettings.KeyBindCT = document.getElementById('keyCtInput').value.trim().toLowerCase() || 'n';
    State.appSettings.KeyBindAll = document.getElementById('keyAllInput').value.trim().toLowerCase() || 'v';
    State.appSettings.KeyBindMute = document.getElementById('keyMuteInput').value.trim().toLowerCase() || 'm';
    State.appSettings.KeyBindSpeedUp = document.getElementById('keySpeedUpInput').value.trim().toLowerCase() || 'shift';
    State.appSettings.KeyBindSlowMo = document.getElementById('keySlowMoInput').value.trim().toLowerCase() || 'ctrl';
    State.appSettings.KeyBindPause = document.getElementById('keyPauseInput').value.trim().toLowerCase() || 'space';
    State.appSettings.KeyBindResetSpeed = document.getElementById('keyResetInput').value.trim().toLowerCase() || 'r';
    State.appSettings.AutoSaveToCs2 = document.getElementById('autoSaveCheckbox').checked;
    State.appSettings.AutoCopyDemoToCs2 = document.getElementById('autoCopyDemoCheckbox').checked;

    if (window.DemoPulseBridge) {
        window.DemoPulseBridge.send("SAVE_SETTINGS", State.appSettings).catch(console.error);
    } else {
        sendToCSharp("SAVE_SETTINGS:" + JSON.stringify(State.appSettings));
    }
    closeSettingsModal();
    updateVoiceConfigUI();
}
