// Window management helpers for Tauri

window.windowManager = {
    _resizeDebounceTimer: null,
    _blazorRef: null,

    // Initialize window management with a Blazor reference for callbacks
    initialize: async function(blazorRef) {
        this._blazorRef = blazorRef;

        if (!window.__TAURI__) {
            console.log("[WindowManager] Not in Tauri environment");
            return false;
        }

        try {
            // Restore saved window size
            await this.restoreWindowSize();

            // Listen for window resize events
            const currentWindow = window.__TAURI__.window.getCurrentWindow();
            await currentWindow.onResized(async (size) => {
                // Debounce to avoid saving on every pixel change
                if (this._resizeDebounceTimer) {
                    clearTimeout(this._resizeDebounceTimer);
                }
                this._resizeDebounceTimer = setTimeout(async () => {
                    await this.saveWindowSize();
                }, 500);
            });

            console.log("[WindowManager] Initialized successfully");
            return true;
        } catch (e) {
            console.error("[WindowManager] Failed to initialize:", e);
            return false;
        }
    },

    // Get current window size
    getWindowSize: async function() {
        if (!window.__TAURI__) {
            return null;
        }

        try {
            const currentWindow = window.__TAURI__.window.getCurrentWindow();
            const size = await currentWindow.innerSize();
            return { width: size.width, height: size.height };
        } catch (e) {
            console.error("[WindowManager] Failed to get window size:", e);
            return null;
        }
    },

    // Set window size
    setWindowSize: async function(width, height) {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            const currentWindow = window.__TAURI__.window.getCurrentWindow();
            const LogicalSize = window.__TAURI__.window.LogicalSize;
            await currentWindow.setSize(new LogicalSize(width, height));
            return true;
        } catch (e) {
            console.error("[WindowManager] Failed to set window size:", e);
            return false;
        }
    },

    // Save current window size to localStorage
    saveWindowSize: async function() {
        const size = await this.getWindowSize();
        if (size) {
            localStorage.setItem('sundy.window.width', size.width.toString());
            localStorage.setItem('sundy.window.height', size.height.toString());
            console.log(`[WindowManager] Saved window size: ${size.width}x${size.height}`);
        }
    },

    // Restore window size from localStorage
    restoreWindowSize: async function() {
        const savedWidth = localStorage.getItem('sundy.window.width');
        const savedHeight = localStorage.getItem('sundy.window.height');

        if (savedWidth && savedHeight) {
            const width = parseInt(savedWidth, 10);
            const height = parseInt(savedHeight, 10);

            // Validate reasonable bounds
            if (width >= 800 && width <= 4000 && height >= 600 && height <= 3000) {
                await this.setWindowSize(width, height);
                console.log(`[WindowManager] Restored window size: ${width}x${height}`);
                return true;
            }
        }

        console.log("[WindowManager] No saved window size to restore");
        return false;
    },

    // Cleanup (call on page unload)
    cleanup: function() {
        if (this._resizeDebounceTimer) {
            clearTimeout(this._resizeDebounceTimer);
        }
        this._blazorRef = null;
    }
};

// Auto-initialize when the page loads (in Tauri environment, main window only)
// DEBUG: Disabled to diagnose gray window issue
/*
document.addEventListener('DOMContentLoaded', async () => {
    if (window.__TAURI__) {
        // Wait a bit for Tauri to fully initialize
        setTimeout(async () => {
            try {
                const currentWindow = window.__TAURI__.window.getCurrentWindow();
                const label = currentWindow.label;
                // Only manage window size for the main window
                if (label === 'main') {
                    await window.windowManager.initialize(null);
                } else {
                    console.log(`[WindowManager] Skipping initialization for window: ${label}`);
                }
            } catch (e) {
                console.error("[WindowManager] Failed to get window label:", e);
            }
        }, 100);
    }
});
*/
