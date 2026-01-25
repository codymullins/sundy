use crate::db::Database;
use crate::notification_window::{self, NotificationWindowState};
use chrono::Utc;
use std::time::Duration;
use tauri::AppHandle;
use tauri_plugin_notification::NotificationExt;

/// Start the background reminder monitor task
pub async fn start_monitor(
    app: AppHandle,
    db: Database,
    notification_state: NotificationWindowState,
) {
    // Initial delay to let the app fully start
    tokio::time::sleep(Duration::from_secs(5)).await;

    loop {
        // Check for due reminders every 30 seconds
        tokio::time::sleep(Duration::from_secs(30)).await;

        if let Err(e) = check_and_notify(&app, &db, &notification_state).await {
            eprintln!("Reminder check error: {}", e);
        }

        // Periodically clean up old events (every check, it's cheap)
        if let Err(e) = cleanup_old_events(&db) {
            eprintln!("Cleanup error: {}", e);
        }
    }
}

/// Get the notification preference from the database
fn get_notification_preference(db: &Database) -> String {
    let conn = db.get_connection();
    let conn = conn.lock().unwrap();

    let result: Result<String, _> = conn.query_row(
        "SELECT value FROM settings WHERE key = 'notification_preference'",
        [],
        |row| row.get(0),
    );

    result.unwrap_or_else(|_| "os_only".to_string())
}

/// Send an OS notification
fn send_os_notification(app: &AppHandle, title: &str, body: &str) -> Result<(), String> {
    app.notification()
        .builder()
        .title(title)
        .body(body)
        .show()
        .map_err(|e| e.to_string())
}

/// Send a notification to the persistent window
fn send_to_persistent_window(
    app: &AppHandle,
    notification_state: &NotificationWindowState,
    event_id: &str,
    title: &str,
    body: &str,
    event_start_time: i64,
) -> Result<(), String> {
    notification_window::add_and_show_notification(
        app,
        notification_state,
        event_id.to_string(),
        title.to_string(),
        body.to_string(),
        event_start_time,
    )?;
    Ok(())
}

/// Check for due reminders and send notifications
async fn check_and_notify(
    app: &AppHandle,
    db: &Database,
    notification_state: &NotificationWindowState,
) -> Result<(), String> {
    let now = Utc::now().timestamp();

    let due_reminders = db
        .get_due_reminders(now)
        .map_err(|e| format!("Failed to get due reminders: {}", e))?;

    // Get the user's notification preference
    let preference = get_notification_preference(db);

    for event in due_reminders {
        // Calculate minutes until event
        let minutes_until = ((event.start_time - now) / 60).max(0);

        let title = if minutes_until == 0 {
            "Event starting now".to_string()
        } else if minutes_until == 1 {
            "Event in 1 minute".to_string()
        } else {
            format!("Event in {} minutes", minutes_until)
        };

        // Format the event time for display
        let event_time = chrono::DateTime::from_timestamp(event.start_time, 0)
            .map(|dt| dt.format("%I:%M %p").to_string())
            .unwrap_or_default();

        let body = if event_time.is_empty() {
            event.title.clone()
        } else {
            format!("{}\n{}", event.title, event_time)
        };

        // Send notification based on user preference
        let mut success = true;
        match preference.as_str() {
            "os_only" => {
                if let Err(e) = send_os_notification(app, &title, &body) {
                    eprintln!("Failed to send OS notification for event {}: {}", event.id, e);
                    success = false;
                }
            }
            "window_only" => {
                if let Err(e) = send_to_persistent_window(
                    app,
                    notification_state,
                    &event.id,
                    &title,
                    &body,
                    event.start_time,
                ) {
                    eprintln!(
                        "Failed to send window notification for event {}: {}",
                        event.id, e
                    );
                    success = false;
                }
            }
            "both" => {
                // Send to both - don't let one failure prevent the other
                if let Err(e) = send_os_notification(app, &title, &body) {
                    eprintln!("Failed to send OS notification for event {}: {}", event.id, e);
                }
                if let Err(e) = send_to_persistent_window(
                    app,
                    notification_state,
                    &event.id,
                    &title,
                    &body,
                    event.start_time,
                ) {
                    eprintln!(
                        "Failed to send window notification for event {}: {}",
                        event.id, e
                    );
                }
            }
            _ => {
                // Default to OS notifications if preference is invalid
                if let Err(e) = send_os_notification(app, &title, &body) {
                    eprintln!("Failed to send OS notification for event {}: {}", event.id, e);
                    success = false;
                }
            }
        }

        // Mark as sent so we don't notify again
        if success {
            if let Err(e) = db.mark_reminder_sent(&event.id) {
                eprintln!("Failed to mark reminder sent for event {}: {}", event.id, e);
            } else {
                println!(
                    "Sent reminder for event: {} ({}) via {}",
                    event.title, event.id, preference
                );
            }
        }
    }

    Ok(())
}

/// Clean up old events from the database
fn cleanup_old_events(db: &Database) -> Result<(), String> {
    let now = Utc::now().timestamp();
    let deleted = db
        .cleanup_old_events(now)
        .map_err(|e| format!("Failed to cleanup old events: {}", e))?;

    if deleted > 0 {
        println!("Cleaned up {} old events", deleted);
    }

    Ok(())
}
