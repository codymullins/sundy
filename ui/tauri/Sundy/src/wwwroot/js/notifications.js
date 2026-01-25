// Web Notifications API wrapper for browser-only mode
// In Tauri mode, notifications are handled by the Rust backend

window.webNotifications = {
    // Check if Web Notifications API is supported
    isSupported: function() {
        return "Notification" in window;
    },

    // Get current permission status
    getPermission: function() {
        if (!this.isSupported()) {
            return "unsupported";
        }
        return Notification.permission;
    },

    // Request notification permission
    requestPermission: async function() {
        if (!this.isSupported()) {
            return "unsupported";
        }

        try {
            const result = await Notification.requestPermission();
            return result;
        } catch (e) {
            console.error("Failed to request notification permission:", e);
            return "denied";
        }
    },

    // Show a notification (browser mode only)
    show: function(title, body, options) {
        if (!this.isSupported()) {
            console.warn("Web Notifications not supported");
            return null;
        }

        if (Notification.permission !== "granted") {
            console.warn("Notification permission not granted");
            return null;
        }

        try {
            const notification = new Notification(title, {
                body: body,
                icon: "/img/icon-192.png",
                tag: options?.tag || undefined,
                requireInteraction: options?.requireInteraction || false,
                silent: options?.silent || false
            });

            // Auto-close after 10 seconds if not interacted with
            if (!options?.requireInteraction) {
                setTimeout(() => notification.close(), 10000);
            }

            // Handle click to focus the app
            notification.onclick = function() {
                window.focus();
                notification.close();
            };

            return notification;
        } catch (e) {
            console.error("Failed to show notification:", e);
            return null;
        }
    },

    // Show a reminder notification with formatted time
    showReminder: function(eventTitle, eventTime, minutesBefore, eventId) {
        let title;
        if (minutesBefore === 0) {
            title = "Event starting now";
        } else if (minutesBefore === 1) {
            title = "Event in 1 minute";
        } else {
            title = `Event in ${minutesBefore} minutes`;
        }

        // Format the event time
        let timeStr = "";
        try {
            const date = new Date(eventTime);
            timeStr = date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
        } catch (e) {
            // Ignore formatting errors
        }

        const body = timeStr ? `${eventTitle}\n${timeStr}` : eventTitle;

        return this.show(title, body, {
            tag: `reminder-${eventId}`,
            requireInteraction: false
        });
    }
};

// Tauri notification sync helpers
window.tauriNotifications = {
    // Sync an event to the Rust backend for reminder scheduling
    syncEvent: async function(id, title, startTime, reminderMinutes) {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("sync_event", {
                id: id,
                title: title,
                startTime: startTime,  // Unix timestamp in seconds
                reminderMinutes: reminderMinutes
            });
            return true;
        } catch (e) {
            console.error("Failed to sync event to Tauri backend:", e);
            return false;
        }
    },

    // Delete an event from the Rust backend
    deleteEvent: async function(id) {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("delete_event", {
                id: id
            });
            return true;
        } catch (e) {
            console.error("Failed to delete event from Tauri backend:", e);
            return false;
        }
    },

    // Check if notification permission is granted (via Rust)
    checkPermission: async function() {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            return await window.__TAURI__.core.invoke("check_notification_permission");
        } catch (e) {
            console.error("Failed to check notification permission:", e);
            return false;
        }
    },

    // Request notification permission from OS (via Rust)
    requestPermission: async function() {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            return await window.__TAURI__.core.invoke("request_notification_permission");
        } catch (e) {
            console.error("Failed to request notification permission:", e);
            return false;
        }
    },

    // Send a test notification (via Rust)
    sendTestNotification: async function() {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("send_test_notification");
            return true;
        } catch (e) {
            console.error("Failed to send test notification:", e);
            return false;
        }
    },

    // =========================================================================
    // Persistent Notification Window
    // =========================================================================

    // Show the persistent notification window
    showNotificationWindow: async function() {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("show_notification_window");
            return true;
        } catch (e) {
            console.error("Failed to show notification window:", e);
            return false;
        }
    },

    // Hide the persistent notification window
    hideNotificationWindow: async function() {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("hide_notification_window");
            return true;
        } catch (e) {
            console.error("Failed to hide notification window:", e);
            return false;
        }
    },

    // Add a notification to the persistent window
    addPersistentNotification: async function(eventId, title, body) {
        if (!window.__TAURI__) {
            return null;
        }

        try {
            return await window.__TAURI__.core.invoke("add_persistent_notification", {
                eventId: eventId,
                title: title,
                body: body
            });
        } catch (e) {
            console.error("Failed to add persistent notification:", e);
            return null;
        }
    },

    // Dismiss a notification from the persistent window
    dismissPersistentNotification: async function(notificationId) {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("dismiss_persistent_notification", {
                notificationId: notificationId
            });
            return true;
        } catch (e) {
            console.error("Failed to dismiss persistent notification:", e);
            return false;
        }
    },

    // Get all notifications in the persistent window
    getPersistentNotifications: async function() {
        if (!window.__TAURI__) {
            return [];
        }

        try {
            return await window.__TAURI__.core.invoke("get_persistent_notifications");
        } catch (e) {
            console.error("Failed to get persistent notifications:", e);
            return [];
        }
    },

    // Get the user's notification display preference
    getNotificationPreference: async function() {
        if (!window.__TAURI__) {
            return "os_only";
        }

        try {
            return await window.__TAURI__.core.invoke("get_notification_preference");
        } catch (e) {
            console.error("Failed to get notification preference:", e);
            return "os_only";
        }
    },

    // Set the user's notification display preference
    setNotificationPreference: async function(preference) {
        if (!window.__TAURI__) {
            return false;
        }

        try {
            await window.__TAURI__.core.invoke("set_notification_preference", {
                preference: preference
            });
            return true;
        } catch (e) {
            console.error("Failed to set notification preference:", e);
            return false;
        }
    }
};

// Helper to check if running in Tauri environment (sync version)
window.isTauriEnvironment = function() {
    return !!window.__TAURI__;
};

// Async version that waits for Tauri to initialize (handles race condition in dev mode)
window.isTauriEnvironmentAsync = async function() {
    // Check immediately
    if (window.__TAURI__) {
        console.log("[DEBUG] isTauriEnvironmentAsync: __TAURI__ found immediately");
        return true;
    }

    console.log("[DEBUG] isTauriEnvironmentAsync: __TAURI__ not found, waiting...");

    // Wait up to 1 second for Tauri to inject __TAURI__
    for (let i = 0; i < 20; i++) {
        await new Promise(resolve => setTimeout(resolve, 50));
        if (window.__TAURI__) {
            console.log(`[DEBUG] isTauriEnvironmentAsync: __TAURI__ found after ${(i+1)*50}ms`);
            return true;
        }
    }

    console.log("[DEBUG] isTauriEnvironmentAsync: __TAURI__ NOT found after 1 second, returning false");
    return false;
};
