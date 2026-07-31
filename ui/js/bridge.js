// Backwards compatibility re-export for bridge
if (typeof sendToCSharp === 'undefined') {
    console.warn('Bridge functions expected from js/core/bridge.js');
}
