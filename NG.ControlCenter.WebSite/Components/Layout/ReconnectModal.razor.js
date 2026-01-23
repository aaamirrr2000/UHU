// Set up event handlers and retry logic
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

let retryCount = 0;
const MAX_RETRIES = 10;
const RETRY_DELAY = 2000;

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show") {
        reconnectModal.showModal();
        retryCount = 0;
    } else if (event.detail.state === "hide") {
        retryCount = 0;
        reconnectModal.close();
    } else if (event.detail.state === "failed") {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        // Reconnect will asynchronously return:
        // - true to mean success
        // - false to mean we reached the server, but it rejected the connection (e.g., unknown circuit ID)
        // - exception to mean we didn't reach the server (this can be sync or async)
        const successful = await Blazor.reconnect();
        if (!successful) {
            // We have been able to reach the server, but the circuit is no longer available.
            // We'll reload the page so the user can continue using the app as quickly as possible.
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else {
                retryCount = 0;
                reconnectModal.close();
            }
        } else {
            retryCount = 0;
            reconnectModal.close();
        }
    } catch (err) {
        // We got an exception, server is currently unavailable
        retryCount++;
        
        if (retryCount < MAX_RETRIES) {
            // Update modal with retry info
            const retryMessage = document.getElementById("retry-message");
            if (retryMessage) {
                retryMessage.textContent = `Reconnecting... (Attempt ${retryCount + 1}/${MAX_RETRIES})`;
            }
            
            // Wait before retrying with exponential backoff
            setTimeout(() => {
                retry();
            }, RETRY_DELAY * Math.pow(1.5, Math.min(retryCount, 3)));
        } else {
            // Max retries exceeded - reload page
            const retryMessage = document.getElementById("retry-message");
            if (retryMessage) {
                retryMessage.textContent = "Connection lost. Please refresh the page.";
            }
            setTimeout(() => {
                location.reload();
            }, 3000);
        }
        
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        }
    } catch {
        location.reload();
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        await retry();
    }
}
