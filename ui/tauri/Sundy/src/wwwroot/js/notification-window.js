// JavaScript for the persistent notification window page
// Handles communication with Blazor and Tauri events

window.notificationWindow = {
    _blazorRef: null,
    _unlisten: null,

    // Initialize the notification window with a Blazor reference
    initialize: async function(blazorRef) {
        this._blazorRef = blazorRef;
        console.log("[NotificationWindow] Initialized with Blazor reference");

        // Listen for notification updates from the Rust backend
        if (window.__TAURI__ && window.__TAURI__.event) {
            try {
                this._unlisten = await window.__TAURI__.event.listen(
                    "notifications-updated",
                    async (event) => {
                        console.log("[NotificationWindow] Received notifications-updated event");
                        if (this._blazorRef) {
                            try {
                                await this._blazorRef.invokeMethodAsync("OnNotificationsUpdated");
                            } catch (e) {
                                console.error("[NotificationWindow] Failed to invoke Blazor method:", e);
                            }
                        }
                    }
                );
                console.log("[NotificationWindow] Listening for notifications-updated events");
            } catch (e) {
                console.error("[NotificationWindow] Failed to set up event listener:", e);
            }
        } else {
            console.warn("[NotificationWindow] Tauri event API not available");
        }
    },

    // Get all current notifications
    getNotifications: async function() {
        if (window.tauriNotifications) {
            return await window.tauriNotifications.getPersistentNotifications();
        }
        return [];
    },

    // Dismiss a notification by ID
    dismiss: async function(notificationId) {
        if (window.tauriNotifications) {
            return await window.tauriNotifications.dismissPersistentNotification(notificationId);
        }
        return false;
    },

    // Clean up event listeners
    cleanup: async function() {
        console.log("[NotificationWindow] Cleaning up...");
        if (this._unlisten) {
            try {
                this._unlisten();
                console.log("[NotificationWindow] Unlistened from events");
            } catch (e) {
                console.error("[NotificationWindow] Failed to unlisten:", e);
            }
            this._unlisten = null;
        }
        this._blazorRef = null;
    },

    // Enable window dragging on an element
    startDrag: async function() {
        if (window.__TAURI__ && window.__TAURI__.window) {
            try {
                const currentWindow = window.__TAURI__.window.getCurrentWindow();
                await currentWindow.startDragging();
            } catch (e) {
                console.error("[NotificationWindow] Failed to start dragging:", e);
            }
        }
    }
};
