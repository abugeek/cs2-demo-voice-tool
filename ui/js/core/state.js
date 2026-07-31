// DemoPulse Encapsulated Event-Driven UI State Store
const Store = (function () {
    let _state = {
        currentMatchData: null,
        currentMode: 'ALL',
        customSlots: {},
        currentSubTab: 'general',
        currentGroupMode: 'team',
        currentTeamFilter: 'ALL',
        appSettings: {
            ConfigFileName: 'demopulse',
            Cs2CfgFolder: '',
            KeyBindT: 'b',
            KeyBindCT: 'n',
            KeyBindAll: 'v',
            KeyBindMute: 'm',
            KeyBindSpeedUp: 'shift',
            KeyBindSlowMo: 'ctrl',
            KeyBindPause: 'space',
            KeyBindResetSpeed: 'r',
            AutoSaveToCs2: true
        }
    };

    const _listeners = [];

    return {
        get currentMatchData() { return _state.currentMatchData; },
        set currentMatchData(val) { _state.currentMatchData = val; this.notify('MATCH_DATA_UPDATED', val); },

        get currentMode() { return _state.currentMode; },
        set currentMode(val) { _state.currentMode = val; this.notify('MODE_UPDATED', val); },

        get customSlots() { return _state.customSlots; },
        set customSlots(val) { _state.customSlots = val; this.notify('SLOTS_UPDATED', val); },

        get currentSubTab() { return _state.currentSubTab; },
        set currentSubTab(val) { _state.currentSubTab = val; this.notify('SUBTAB_UPDATED', val); },

        get currentGroupMode() { return _state.currentGroupMode; },
        set currentGroupMode(val) { _state.currentGroupMode = val; this.notify('GROUPMODE_UPDATED', val); },

        get currentTeamFilter() { return _state.currentTeamFilter; },
        set currentTeamFilter(val) { _state.currentTeamFilter = val; this.notify('TEAMFILTER_UPDATED', val); },

        get appSettings() { return _state.appSettings; },
        set appSettings(val) { _state.appSettings = val; this.notify('SETTINGS_UPDATED', val); },

        subscribe(listener) {
            if (typeof listener === 'function') {
                _listeners.push(listener);
            }
        },

        notify(event, data) {
            _listeners.forEach(fn => {
                try { fn(event, data); } catch (e) { console.error('Store listener error:', e); }
            });
        }
    };
})();

// Global alias for backwards compatibility with existing UI renderers
const State = Store;

// Global HTML entity escaper to prevent Stored XSS from untrusted demo metadata/player names
function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}
