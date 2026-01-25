use chrono::Utc;
use serde::{Deserialize, Serialize};
use std::sync::{Arc, Mutex};
use tauri::{AppHandle, Emitter, Manager};

/// A persistent notification displayed in the notification window
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PersistentNotification {
    pub id: String,
    pub event_id: String,
    pub title: String,
    pub body: String,
    pub created_at: i64,
    /// Event start time as Unix timestamp (seconds)
    pub event_start_time: i64,
}

/// Shared state for the persistent notification window
pub struct NotificationWindowState {
    notifications: Arc<Mutex<Vec<PersistentNotification>>>,
}

impl NotificationWindowState {
    pub fn new() -> Self {
        Self {
            notifications: Arc::new(Mutex::new(Vec::new())),
        }
    }

    /// Add a notification and return its ID
    pub fn add_notification(
        &self,
        event_id: String,
        title: String,
        body: String,
        event_start_time: i64,
    ) -> String {
        let notification = PersistentNotification {
            id: uuid::Uuid::new_v4().to_string(),
            event_id,
            title,
            body,
            created_at: Utc::now().timestamp(),
            event_start_time,
        };
        let id = notification.id.clone();

        let mut notifications = self.notifications.lock().unwrap();
        notifications.push(notification);

        id
    }

    /// Dismiss a notification by ID, returns true if found and removed
    pub fn dismiss_notification(&self, notification_id: &str) -> bool {
        let mut notifications = self.notifications.lock().unwrap();
        let len_before = notifications.len();
        notifications.retain(|n| n.id != notification_id);
        notifications.len() < len_before
    }

    /// Get all current notifications
    pub fn get_all_notifications(&self) -> Vec<PersistentNotification> {
        let notifications = self.notifications.lock().unwrap();
        notifications.clone()
    }

    /// Check if there are any notifications
    pub fn has_notifications(&self) -> bool {
        let notifications = self.notifications.lock().unwrap();
        !notifications.is_empty()
    }

    /// Clear all notifications
    pub fn clear_all(&self) {
        let mut notifications = self.notifications.lock().unwrap();
        notifications.clear();
    }
}

impl Clone for NotificationWindowState {
    fn clone(&self) -> Self {
        Self {
            notifications: Arc::clone(&self.notifications),
        }
    }
}

impl Default for NotificationWindowState {
    fn default() -> Self {
        Self::new()
    }
}

/// Show the notification window positioned in the bottom-right corner
pub fn show_notification_window(app: &AppHandle) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("notification-window") {
        // Position window in bottom-right corner of primary monitor
        if let Ok(Some(monitor)) = window.primary_monitor() {
            let monitor_size = monitor.size();
            let monitor_position = monitor.position();
            let window_size = window.outer_size().unwrap_or(tauri::PhysicalSize::new(380, 420));

            // Calculate position with padding from edges
            let padding = 20;
            let x = monitor_position.x + (monitor_size.width as i32) - (window_size.width as i32) - padding;
            let y = monitor_position.y + (monitor_size.height as i32) - (window_size.height as i32) - padding;

            let position = tauri::PhysicalPosition::new(x, y);
            let _ = window.set_position(position);
        }

        window.show().map_err(|e| e.to_string())?;
        // Don't steal focus from the user's current work
        // window.set_focus().map_err(|e| e.to_string())?;
    } else {
        return Err("Notification window not found".to_string());
    }
    Ok(())
}

/// Hide the notification window
pub fn hide_notification_window(app: &AppHandle) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("notification-window") {
        window.hide().map_err(|e| e.to_string())?;
    }
    Ok(())
}

/// Emit an event to notify the window that notifications have changed
pub fn emit_notifications_updated(app: &AppHandle) -> Result<(), String> {
    app.emit_to("notification-window", "notifications-updated", ())
        .map_err(|e| e.to_string())
}

/// Add a notification and show the window if needed
pub fn add_and_show_notification(
    app: &AppHandle,
    state: &NotificationWindowState,
    event_id: String,
    title: String,
    body: String,
    event_start_time: i64,
) -> Result<String, String> {
    let notification_id = state.add_notification(event_id, title, body, event_start_time);

    // Show the window if it was hidden
    show_notification_window(app)?;

    // Notify the window to refresh its content
    emit_notifications_updated(app)?;

    Ok(notification_id)
}

/// Dismiss a notification and hide the window if empty
pub fn dismiss_and_maybe_hide(
    app: &AppHandle,
    state: &NotificationWindowState,
    notification_id: &str,
) -> Result<bool, String> {
    let dismissed = state.dismiss_notification(notification_id);

    if dismissed {
        // Notify the window to refresh
        emit_notifications_updated(app)?;

        // Hide window if no more notifications
        if !state.has_notifications() {
            hide_notification_window(app)?;
        }
    }

    Ok(dismissed)
}
