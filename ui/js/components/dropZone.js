// DemoPulse Drag & Drop Component
function initDragAndDrop() {
    const dropZone = document.getElementById('dropZone');
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        document.body.addEventListener(eventName, e => e.preventDefault(), false);
        window.addEventListener(eventName, e => e.preventDefault(), false);
    });

    document.body.addEventListener('dragover', (e) => {
        e.preventDefault();
        if (dropZone) dropZone.classList.add('dragover');
    });

    document.body.addEventListener('dragleave', (e) => {
        e.preventDefault();
        if (dropZone) dropZone.classList.remove('dragover');
    });

    document.body.addEventListener('drop', (e) => {
        e.preventDefault();
        if (dropZone) dropZone.classList.remove('dragover');
        const files = e.dataTransfer ? e.dataTransfer.files : null;
        if (files && files.length > 0) {
            const file = files[0];
            const name = file.path || file.name;
            if (name && name.toLowerCase().endsWith('.dem')) {
                requestDemoParse(name);
            } else {
                showError("Only .dem files are supported.");
            }
        }
    });
}

function triggerFileSelect() {
    if (window.DemoPulseBridge && window.chrome && window.chrome.webview) {
        window.DemoPulseBridge.send("SELECT_FILE", null).catch(console.error);
    } else if (!sendToCSharp("SELECT_FILE")) {
        document.getElementById('fileInput').click();
    }
}

function handleFileSelect(event) {
    const files = event.target.files;
    if (files.length > 0) {
        const file = files[0];
        const name = file.path || file.name;
        if (name && name.toLowerCase().endsWith('.dem')) {
            requestDemoParse(name);
        } else {
            showError("Only .dem files are supported.");
        }
    }
}
