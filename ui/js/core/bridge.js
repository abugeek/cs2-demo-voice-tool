// DemoPulse C# WebView2 Interop Bridge with Typed JSON Contracts & Correlation IDs
(function () {
    let requestIdCounter = 0;
    const pendingRequests = new Map();

    window.DemoPulseBridge = {
        send: function (command, payload = null) {
            return new Promise((resolve, reject) => {
                const id = `req_${Date.now()}_${++requestIdCounter}`;
                const envelope = { id, command, payload };

                if (window.chrome && window.chrome.webview) {
                    pendingRequests.set(id, { resolve, reject });
                    window.chrome.webview.postMessage(JSON.stringify(envelope));
                } else {
                    reject(new Error("WebView2 interop bridge not available"));
                }
            });
        }
    };

    window.sendToCSharp = function (message) {
        if (window.chrome && window.chrome.webview) {
            if (typeof message === 'object') {
                window.chrome.webview.postMessage(JSON.stringify(message));
            } else {
                window.chrome.webview.postMessage(message);
            }
            return true;
        }
        return false;
    };

    // Helper functions for UI
    window.requestDemoParse = function (filePath) {
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("PARSE_DEMO", { filePath }).catch(err => showError(err.message));
        } else {
            fetchDemoDataFallback(filePath);
        }
    };

    window.requestSettings = function () {
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("GET_SETTINGS", null).catch(console.error);
        } else {
            sendToCSharp("GET_SETTINGS");
        }
    };

    window.launchCS2 = function () {
        const filePath = State.currentMatchData && State.currentMatchData.meta ? State.currentMatchData.meta.filePath : "";
        const { tMask, ctMask } = typeof calculateTeamBitmasks === 'function' ? calculateTeamBitmasks() : { tMask: "31", ctMask: "992" };
        const payload = { filePath, tMask, ctMask };
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("LAUNCH_CS2", payload).catch(console.error);
        } else {
            sendToCSharp(`LAUNCH_CS2:${filePath}:${tMask}:${ctMask}`);
        }
    };

    window.openDemoFolder = function () {
        const filePath = State.currentMatchData && State.currentMatchData.meta ? State.currentMatchData.meta.filePath : "";
        if (!filePath) return;
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("OPEN_DEMO_FOLDER", { filePath }).catch(console.error);
        } else {
            sendToCSharp("OPEN_DEMO_FOLDER:" + filePath);
        }
    };

    window.renameCurrentDemo = function (newName) {
        const currentPath = State.currentMatchData && State.currentMatchData.meta ? State.currentMatchData.meta.filePath : "";
        if (!currentPath) return Promise.reject(new Error("No demo file currently loaded"));
        const payload = { currentPath, newName };
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            return window.DemoPulseBridge.send("RENAME_DEMO", payload);
        } else {
            sendToCSharp("RENAME_DEMO:" + currentPath + ":" + newName);
            return Promise.resolve();
        }
    };

    window.copyPlaydemoCmd = function () {
        const fileName = State.currentMatchData && State.currentMatchData.meta ? State.currentMatchData.meta.fileName : "";
        if (!fileName) return;
        const nameNoExt = fileName.replace(/\.dem$/i, '');
        const cmd = `playdemo ${nameNoExt}`;
        navigator.clipboard.writeText(cmd).then(() => {
            alert(`CS2 console command copied to clipboard!\n${cmd}`);
        }).catch(() => {
            alert(`CS2 console command:\n${cmd}`);
        });
    };

    window.browseCs2Folder = function () {
        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("BROWSE_CS2_FOLDER", null).catch(console.error);
        } else {
            sendToCSharp("BROWSE_CS2_FOLDER");
        }
    };

    window.exportVoiceConfig = function (mode) {
        const maskDec = (mode === 'ALL') ? "18446744073709551615" : calculateBitmaskFromSlots();
        const { tMask, ctMask } = calculateTeamBitmasks();
        const payload = { mode, customMask: maskDec, tMask, ctMask };

        if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
            window.DemoPulseBridge.send("EXPORT_VOICE_CFG", payload).catch(err => {
                alert(`Exporting CS2 .cfg for ${mode} mode (mask: ${maskDec}).`);
            });
        } else if (!sendToCSharp(`EXPORT_VOICE_CFG:${mode}:${maskDec}:${tMask}:${ctMask}`)) {
            alert(`Exporting CS2 .cfg for ${mode} mode (mask: ${maskDec}).`);
        }
    };

    window.fetchDemoDataFallback = function (filePath) {
        showError('Real demo parsing requires the DemoPulse desktop app. Drop a .dem file in the app window.');
    };

    const chunkBuffers = new Map();

    function handleIpcParsedMessage(parsed) {
        if (!parsed || typeof parsed !== 'object' || !parsed.type) return;

        // Check pending Promise correlation ID
        if (parsed.id && pendingRequests.has(parsed.id)) {
            const { resolve, reject } = pendingRequests.get(parsed.id);
            pendingRequests.delete(parsed.id);
            if (parsed.success) {
                resolve(parsed.payload);
            } else {
                reject(new Error(parsed.error || 'Request failed'));
            }
        }

        // Handle broadcast events
        const msgType = parsed.type;
        if (msgType === 'DEMO_PARSING_START') {
            showLoadingState();
        } else if (msgType === 'DEMO_DATA') {
            renderFullDemo(parsed.payload);
        } else if (msgType === 'DEMO_RENAMED' && parsed.payload && parsed.payload.success) {
            if (State.currentMatchData && State.currentMatchData.meta) {
                State.currentMatchData.meta.filePath = parsed.payload.newPath;
                State.currentMatchData.meta.fileName = parsed.payload.newFileName;
                const nameElem = document.getElementById('matchFileName');
                if (nameElem) nameElem.innerText = parsed.payload.newFileName;
            }
        } else if (msgType === 'DEMO_PARSE_ERROR') {
            showError(parsed.error || 'Failed to parse demo');
        } else if (msgType === 'SETTINGS_DATA') {
            populateSettingsModal(parsed.payload);
        } else if (msgType === 'SETTINGS_SAVED') {
            updateVoiceConfigUI();
        } else if (msgType === 'VOICE_CFG_RESULT' && parsed.payload && parsed.payload.configText) {
            const liveCmd = document.getElementById('liveConsoleCmd');
            if (liveCmd) liveCmd.innerText = parsed.payload.configText;
        } else if (msgType === 'AUTOSAVE_ERROR') {
            showError(parsed.error || 'CS2 config auto-save failed');
        }
    }

    // C# Message Listener Setup
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', event => {
            if (event.data && typeof event.data === 'object' && event.data.type) {
                handleIpcParsedMessage(event.data);
                return;
            }

            if (typeof event.data === 'string') {
                let parsed = null;
                try {
                    parsed = JSON.parse(event.data);
                } catch (e) {
                    // Raw string legacy message
                }

                if (parsed && typeof parsed === 'object' && parsed.type) {
                    if (parsed.type === 'IPC_CHUNK') {
                        const { chunkId, index, total, data } = parsed;
                        if (!chunkBuffers.has(chunkId)) {
                            chunkBuffers.set(chunkId, new Array(total));
                        }
                        const buffer = chunkBuffers.get(chunkId);
                        buffer[index] = data;

                        let isComplete = true;
                        for (let i = 0; i < total; i++) {
                            if (buffer[i] === undefined) {
                                isComplete = false;
                                break;
                            }
                        }

                        if (isComplete) {
                            chunkBuffers.delete(chunkId);
                            const fullJsonStr = buffer.join('');
                            try {
                                const assembledResponse = JSON.parse(fullJsonStr);
                                handleIpcParsedMessage(assembledResponse);
                            } catch (err) {
                                console.error('Failed to parse reassembled IPC payload:', err);
                            }
                        }
                        return;
                    }

                    handleIpcParsedMessage(parsed);
                    return;
                }

                // Legacy raw string handling
                const data = event.data;
                if (data === 'DEMO_PARSING_START') {
                    showLoadingState();
                } else if (data.startsWith('DEMO_DATA:')) {
                    const jsonStr = data.substring(10);
                    try {
                        const matchData = JSON.parse(jsonStr);
                        renderFullDemo(matchData);
                    } catch (err) {
                        showError('Failed to parse demo data: ' + err.message);
                    }
                } else if (data.startsWith('DEMO_PARSE_ERROR:')) {
                    const errMsg = data.substring('DEMO_PARSE_ERROR:'.length);
                    showError(errMsg);
                } else if (data.startsWith('AUTOSAVE_ERROR:')) {
                    const errMsg = data.substring('AUTOSAVE_ERROR:'.length);
                    showError(errMsg);
                } else if (data.startsWith('OPEN_DEMO:')) {
                    const filePath = data.substring(10);
                    requestDemoParse(filePath);
                } else if (data.startsWith('SETTINGS_DATA:')) {
                    const jsonStr = data.substring('SETTINGS_DATA:'.length);
                    try {
                        const s = JSON.parse(jsonStr);
                        populateSettingsModal(s);
                    } catch (err) { console.error(err); }
                } else if (data === 'SETTINGS_SAVED') {
                    updateVoiceConfigUI();
                }
            }
        });
    }
})();
