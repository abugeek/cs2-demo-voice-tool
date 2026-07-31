// DemoPulse Interactive Keybind Recorder & Mapper
let activeKeyRecorderElem = null;
let activeKeyHandler = null;

function mapBrowserKeyToCs2Key(e) {
    const code = e.code || '';
    const key = (e.key || '').toLowerCase();

    if (code.startsWith('Key') && code.length === 4) return code.substring(3).toLowerCase();
    if (code.startsWith('Digit') && code.length === 6) return code.substring(5);
    if (code.startsWith('Numpad') && code.length === 7) return 'kp_' + code.substring(6).toLowerCase();

    if (code === 'ControlLeft' || code === 'ControlRight' || key === 'control') return 'ctrl';
    if (code === 'ShiftLeft' || code === 'ShiftRight' || key === 'shift') return 'shift';
    if (code === 'AltLeft' || code === 'AltRight' || key === 'alt') return 'alt';
    if (code === 'Space' || key === ' ' || key === 'space') return 'space';
    if (code === 'Tab' || key === 'tab') return 'tab';
    if (code === 'CapsLock' || key === 'capslock') return 'capslock';
    if (code === 'Backspace' || key === 'backspace') return 'backspace';
    if (code === 'Enter' || key === 'enter') return 'enter';
    if (code === 'Escape' || key === 'escape') return 'escape';

    if (code === 'ArrowUp' || key === 'arrowup') return 'uparrow';
    if (code === 'ArrowDown' || key === 'arrowdown') return 'downarrow';
    if (code === 'ArrowLeft' || key === 'arrowleft') return 'leftarrow';
    if (code === 'ArrowRight' || key === 'arrowright') return 'rightarrow';

    if (code.startsWith('F') && code.length <= 3) return code.toLowerCase();

    const clean = key.replace(/[^a-z0-9_]/g, '');
    return (clean.length > 0 && clean.length <= 15) ? clean : 'b';
}

function checkAndResolveHotkeyConflicts(newKey, currentId) {
    const allIds = [
        'keyTInput', 'keyCtInput', 'keyAllInput', 'keyMuteInput',
        'keySpeedUpInput', 'keySlowMoInput', 'keyPauseInput', 'keyResetInput'
    ];

    allIds.forEach(id => {
        if (id !== currentId) {
            const elem = document.getElementById(id);
            if (elem && elem.value.trim().toLowerCase() === newKey.toLowerCase()) {
                elem.value = '[ unassigned ]';
                elem.style.borderColor = 'var(--accent-red)';
                setTimeout(() => { elem.style.borderColor = 'var(--panel-border)'; }, 2000);
                showConflictNotice(`Hotkey '${newKey.toUpperCase()}' was unassigned from another field to prevent duplicate bindings.`);
            }
        }
    });
}

function showConflictNotice(msg) {
    let notice = document.getElementById('keyConflictNotice');
    if (!notice) {
        notice = document.createElement('div');
        notice.id = 'keyConflictNotice';
        notice.style.cssText = 'position: fixed; bottom: 20px; right: 20px; background: rgba(255,71,87,0.95); color: #fff; padding: 10px 16px; border-radius: 6px; font-size: 12px; font-weight: 600; z-index: 10000; box-shadow: 0 4px 12px rgba(0,0,0,0.4); border: 1px solid var(--accent-red); transition: opacity 0.3s;';
        document.body.appendChild(notice);
    }
    notice.innerText = '⚠️ ' + msg;
    notice.style.opacity = '1';
    notice.style.display = 'block';
    setTimeout(() => {
        notice.style.opacity = '0';
        setTimeout(() => { notice.style.display = 'none'; }, 300);
    }, 3500);
}

function startKeyRecording(elemId) {
    const elem = document.getElementById(elemId);
    if (!elem) return;

    if (activeKeyRecorderElem && activeKeyHandler) {
        window.removeEventListener('keydown', activeKeyHandler, true);
        activeKeyRecorderElem.classList.remove('recording');
        if (activeKeyRecorderElem.dataset.savedValue) {
            activeKeyRecorderElem.value = activeKeyRecorderElem.dataset.savedValue;
        }
    }

    activeKeyRecorderElem = elem;
    elem.dataset.savedValue = elem.value;
    elem.value = '[ Press a key... ]';
    elem.classList.add('recording');

    activeKeyHandler = (e) => {
        e.preventDefault();
        e.stopPropagation();

        if (e.key === 'Escape') {
            elem.value = elem.dataset.savedValue || 'b';
        } else {
            const cs2Key = mapBrowserKeyToCs2Key(e);
            checkAndResolveHotkeyConflicts(cs2Key, elemId);
            elem.value = cs2Key;
        }

        window.removeEventListener('keydown', activeKeyHandler, true);
        elem.classList.remove('recording');
        activeKeyRecorderElem = null;
        activeKeyHandler = null;
    };

    window.addEventListener('keydown', activeKeyHandler, true);
}

function resetHotkeysToDefault() {
    if (activeKeyRecorderElem && activeKeyHandler) {
        window.removeEventListener('keydown', activeKeyHandler, true);
        activeKeyRecorderElem.classList.remove('recording');
        activeKeyRecorderElem = null;
        activeKeyHandler = null;
    }

    document.getElementById('keyTInput').value = 'b';
    document.getElementById('keyCtInput').value = 'n';
    document.getElementById('keyAllInput').value = 'v';
    document.getElementById('keyMuteInput').value = 'm';

    document.getElementById('keySpeedUpInput').value = 'shift';
    document.getElementById('keySlowMoInput').value = 'ctrl';
    document.getElementById('keyPauseInput').value = 'space';
    document.getElementById('keyResetInput').value = 'r';
}
