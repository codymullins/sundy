mod db;
mod notification_window;
mod reminder_monitor;

use db::{Database, ReminderEvent};
use notification_window::{NotificationWindowState, PersistentNotification};
use serde::{Deserialize, Serialize};
use tauri::{Manager, State, WebviewUrl, WebviewWindowBuilder};

/// Apply macOS-specific styling for transparent rounded window
#[cfg(target_os = "macos")]
fn apply_macos_transparency(window: &tauri::WebviewWindow) {
    use cocoa::appkit::{NSColor, NSWindow};
    use cocoa::base::{id, nil, NO, YES};
    use objc::{msg_send, sel, sel_impl};

    unsafe {
        // Get the raw NSWindow pointer from the webview window
        if let Ok(raw_window) = window.ns_window() {
            let ns_window = raw_window as id;

            // Make window background transparent
            let clear_color = NSColor::clearColor(nil);
            ns_window.setBackgroundColor_(clear_color);

            // Don't hide on deactivate
            ns_window.setHidesOnDeactivate_(NO);

            // Set opaque to NO for transparency
            let _: () = msg_send![ns_window, setOpaque: NO];

            // Get content view and apply corner radius
            let content_view: id = ns_window.contentView();

            // Enable layer-backed view for corner radius
            let _: () = msg_send![content_view, setWantsLayer: YES];
            let layer: id = msg_send![content_view, layer];
            if layer != nil {
                let _: () = msg_send![layer, setCornerRadius: 12.0_f64];
                let _: () = msg_send![layer, setMasksToBounds: YES];
            }
        }
    }
}

#[cfg(not(target_os = "macos"))]
fn apply_macos_transparency(_window: &tauri::WebviewWindow) {
    // No-op on non-macOS platforms
}

/// Application state shared across commands
pub struct AppState {
    pub db: Database,
    pub notification_window: NotificationWindowState,
}

// OAuth token response from Microsoft
#[derive(Debug, Serialize, Deserialize)]
pub struct TokenResponse {
    pub access_token: String,
    pub token_type: String,
    pub expires_in: u64,
    pub scope: String,
    pub refresh_token: Option<String>,
}

// Error response from Microsoft
#[derive(Debug, Serialize, Deserialize)]
pub struct OAuthError {
    pub error: String,
    pub error_description: Option<String>,
}

const CLIENT_ID: &str = "45770f2c-6da1-47f0-9ee0-16ac86df3a10";
const TOKEN_ENDPOINT: &str = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hello, {}! You've been greeted from Rust!", name)
}

/// Exchange authorization code for tokens (bypasses CORS)
#[tauri::command]
async fn exchange_oauth_code(
    code: String,
    code_verifier: String,
    redirect_uri: String,
) -> Result<TokenResponse, String> {
    let client = reqwest::Client::new();

    let params = [
        ("client_id", CLIENT_ID),
        ("grant_type", "authorization_code"),
        ("code", &code),
        ("redirect_uri", &redirect_uri),
        ("code_verifier", &code_verifier),
    ];

    let response = client
        .post(TOKEN_ENDPOINT)
        .form(&params)
        .send()
        .await
        .map_err(|e| format!("Request failed: {}", e))?;

    if response.status().is_success() {
        response
            .json::<TokenResponse>()
            .await
            .map_err(|e| format!("Failed to parse token response: {}", e))
    } else {
        let error: OAuthError = response
            .json()
            .await
            .unwrap_or(OAuthError {
                error: "unknown_error".to_string(),
                error_description: Some("Failed to parse error response".to_string()),
            });
        Err(format!(
            "{}: {}",
            error.error,
            error.error_description.unwrap_or_default()
        ))
    }
}

/// Refresh access token using refresh token (bypasses CORS)
#[tauri::command]
async fn refresh_oauth_token(refresh_token: String) -> Result<TokenResponse, String> {
    let client = reqwest::Client::new();

    let params = [
        ("client_id", CLIENT_ID),
        ("grant_type", "refresh_token"),
        ("refresh_token", refresh_token.as_str()),
        ("scope", "user.read Calendars.ReadWrite offline_access"),
    ];

    let response = client
        .post(TOKEN_ENDPOINT)
        .form(&params)
        .send()
        .await
        .map_err(|e| format!("Request failed: {}", e))?;

    if response.status().is_success() {
        response
            .json::<TokenResponse>()
            .await
            .map_err(|e| format!("Failed to parse token response: {}", e))
    } else {
        let error: OAuthError = response
            .json()
            .await
            .unwrap_or(OAuthError {
                error: "unknown_error".to_string(),
                error_description: Some("Failed to parse error response".to_string()),
            });
        Err(format!(
            "{}: {}",
            error.error,
            error.error_description.unwrap_or_default()
        ))
    }
}

/// Sync an event from the frontend for reminder scheduling
/// If the reminder time has already passed, triggers notification immediately
#[tauri::command]
async fn sync_event(
    state: State<'_, AppState>,
    app: tauri::AppHandle,
    id: String,
    title: String,
    start_time: i64,
    reminder_minutes: i32,
) -> Result<(), String> {
    let event = ReminderEvent {
        id: id.clone(),
        title: title.clone(),
        start_time,
        reminder_minutes,
    };

    state
        .db
        .upsert_event(&event)
        .map_err(|e| format!("Failed to sync event: {}", e))?;

    // Check if reminder should fire immediately
    let now = chrono::Utc::now().timestamp();
    let reminder_time = start_time - (reminder_minutes as i64 * 60);

    // If reminder time has passed but event hasn't been over for more than 5 minutes
    if reminder_time <= now && start_time > (now - 300) {
        // Check if reminder was already sent
        let already_sent = {
            let conn = state.db.get_connection();
            let conn = conn.lock().unwrap();
            let result: Result<i32, _> = conn.query_row(
                "SELECT reminder_sent FROM events WHERE id = ?1",
                [&id],
                |row| row.get(0),
            );
            result.unwrap_or(0) == 1
        };

        if !already_sent {
            // Get notification preference and send immediately
            let preference = {
                let conn = state.db.get_connection();
                let conn = conn.lock().unwrap();
                let result: Result<String, _> = conn.query_row(
                    "SELECT value FROM settings WHERE key = 'notification_preference'",
                    [],
                    |row| row.get(0),
                );
                result.unwrap_or_else(|_| "os_only".to_string())
            };

            // Calculate display text
            let minutes_until = ((start_time - now) / 60).max(0);
            let notif_title = if minutes_until == 0 {
                "Event starting now".to_string()
            } else if minutes_until == 1 {
                "Event in 1 minute".to_string()
            } else {
                format!("Event in {} minutes", minutes_until)
            };

            let event_time = chrono::DateTime::from_timestamp(start_time, 0)
                .map(|dt| dt.format("%I:%M %p").to_string())
                .unwrap_or_default();

            let body = if event_time.is_empty() {
                title.clone()
            } else {
                format!("{}\n{}", title, event_time)
            };

            // Send based on preference
            match preference.as_str() {
                "window_only" => {
                    let _ = notification_window::add_and_show_notification(
                        &app,
                        &state.notification_window,
                        id.clone(),
                        notif_title,
                        body,
                        start_time,
                    );
                }
                "both" => {
                    use tauri_plugin_notification::NotificationExt;
                    let _ = app
                        .notification()
                        .builder()
                        .title(&notif_title)
                        .body(&body)
                        .show();
                    let _ = notification_window::add_and_show_notification(
                        &app,
                        &state.notification_window,
                        id.clone(),
                        notif_title,
                        body,
                        start_time,
                    );
                }
                _ => {
                    // os_only or default
                    use tauri_plugin_notification::NotificationExt;
                    let _ = app
                        .notification()
                        .builder()
                        .title(&notif_title)
                        .body(&body)
                        .show();
                }
            }

            // Mark as sent
            let _ = state.db.mark_reminder_sent(&id);
            println!("Immediate reminder sent for event: {} ({})", title, id);
        }
    }

    Ok(())
}

/// Delete an event from the reminder database
#[tauri::command]
async fn delete_event(state: State<'_, AppState>, id: String) -> Result<(), String> {
    state
        .db
        .delete_event(&id)
        .map_err(|e| format!("Failed to delete event: {}", e))?;

    Ok(())
}

/// Check if notification permission is granted
#[tauri::command]
async fn check_notification_permission(app: tauri::AppHandle) -> Result<bool, String> {
    use tauri_plugin_notification::NotificationExt;
    app.notification()
        .permission_state()
        .map(|state| state == tauri_plugin_notification::PermissionState::Granted)
        .map_err(|e| e.to_string())
}

/// Request notification permission from the OS
#[tauri::command]
async fn request_notification_permission(app: tauri::AppHandle) -> Result<bool, String> {
    use tauri_plugin_notification::NotificationExt;
    app.notification()
        .request_permission()
        .map(|state| state == tauri_plugin_notification::PermissionState::Granted)
        .map_err(|e| e.to_string())
}

/// Send a test notification to verify the system is working
/// Respects the user's notification preference (os_only, window_only, or both)
#[tauri::command]
async fn send_test_notification(
    state: State<'_, AppState>,
    app: tauri::AppHandle,
) -> Result<(), String> {
    use tauri_plugin_notification::NotificationExt;

    println!("[DEBUG] send_test_notification called");

    // Get notification preference from database
    let preference = {
        let conn = state.db.get_connection();
        let conn = conn.lock().unwrap();
        let result: Result<String, _> = conn.query_row(
            "SELECT value FROM settings WHERE key = 'notification_preference'",
            [],
            |row| row.get(0),
        );
        result.unwrap_or_else(|_| "os_only".to_string())
    };

    println!("[DEBUG] Notification preference: {}", preference);

    let title = "Test Notification".to_string();
    let body = "Notifications are working!".to_string();
    let now = chrono::Utc::now().timestamp();

    match preference.as_str() {
        "window_only" => {
            // Send to persistent window only
            let _ = notification_window::add_and_show_notification(
                &app,
                &state.notification_window,
                "test".to_string(),
                title,
                body,
                now,
            );
            println!("[DEBUG] Test notification sent to persistent window");
            Ok(())
        }
        "both" => {
            // Send to both OS and persistent window
            let os_result = app
                .notification()
                .builder()
                .title(&title)
                .body(&body)
                .show();

            let _ = notification_window::add_and_show_notification(
                &app,
                &state.notification_window,
                "test".to_string(),
                title,
                body,
                now,
            );

            println!("[DEBUG] Test notification sent to both OS and persistent window");
            os_result.map_err(|e| e.to_string())
        }
        _ => {
            // "os_only" or default - current behavior
            let result = app
                .notification()
                .builder()
                .title(&title)
                .body(&body)
                .show();

            match &result {
                Ok(_) => println!("[DEBUG] OS notification show() succeeded"),
                Err(e) => println!("[DEBUG] OS notification show() failed: {:?}", e),
            }

            result.map_err(|e| e.to_string())
        }
    }
}

// ============================================================================
// Persistent Notification Window Commands
// ============================================================================

/// Show the persistent notification window
#[tauri::command]
async fn show_notification_window(app: tauri::AppHandle) -> Result<(), String> {
    notification_window::show_notification_window(&app)
}

/// Hide the persistent notification window
#[tauri::command]
async fn hide_notification_window(app: tauri::AppHandle) -> Result<(), String> {
    notification_window::hide_notification_window(&app)
}

/// Add a notification to the persistent window
#[tauri::command]
async fn add_persistent_notification(
    state: State<'_, AppState>,
    app: tauri::AppHandle,
    event_id: String,
    title: String,
    body: String,
    event_start_time: i64,
) -> Result<String, String> {
    notification_window::add_and_show_notification(
        &app,
        &state.notification_window,
        event_id,
        title,
        body,
        event_start_time,
    )
}

/// Dismiss a notification from the persistent window
#[tauri::command]
async fn dismiss_persistent_notification(
    state: State<'_, AppState>,
    app: tauri::AppHandle,
    notification_id: String,
) -> Result<bool, String> {
    notification_window::dismiss_and_maybe_hide(&app, &state.notification_window, &notification_id)
}

/// Get all notifications in the persistent window
#[tauri::command]
async fn get_persistent_notifications(
    state: State<'_, AppState>,
) -> Result<Vec<PersistentNotification>, String> {
    Ok(state.notification_window.get_all_notifications())
}

/// Get the user's notification display preference
#[tauri::command]
async fn get_notification_preference(state: State<'_, AppState>) -> Result<String, String> {
    // Read from the settings table in the database
    let conn = state.db.get_connection();
    let conn = conn.lock().unwrap();

    // Ensure settings table exists
    conn.execute(
        "CREATE TABLE IF NOT EXISTS settings (
            key TEXT PRIMARY KEY,
            value TEXT NOT NULL
        )",
        [],
    )
    .map_err(|e| e.to_string())?;

    let result: Result<String, _> = conn.query_row(
        "SELECT value FROM settings WHERE key = 'notification_preference'",
        [],
        |row| row.get(0),
    );

    match result {
        Ok(pref) => Ok(pref),
        Err(_) => Ok("os_only".to_string()), // Default to OS notifications
    }
}

/// Set the user's notification display preference
#[tauri::command]
async fn set_notification_preference(
    state: State<'_, AppState>,
    preference: String,
) -> Result<(), String> {
    // Validate preference
    if !["os_only", "window_only", "both"].contains(&preference.as_str()) {
        return Err("Invalid preference. Must be 'os_only', 'window_only', or 'both'".to_string());
    }

    let conn = state.db.get_connection();
    let conn = conn.lock().unwrap();

    // Ensure settings table exists
    conn.execute(
        "CREATE TABLE IF NOT EXISTS settings (
            key TEXT PRIMARY KEY,
            value TEXT NOT NULL
        )",
        [],
    )
    .map_err(|e| e.to_string())?;

    conn.execute(
        "INSERT INTO settings (key, value) VALUES ('notification_preference', ?1)
         ON CONFLICT(key) DO UPDATE SET value = excluded.value",
        [&preference],
    )
    .map_err(|e| e.to_string())?;

    Ok(())
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_notification::init())
        .plugin(tauri_plugin_window_state::Builder::default().build())
        .setup(|app| {
            // Initialize the reminder database
            let app_data_dir = app
                .path()
                .app_data_dir()
                .expect("Failed to get app data directory");

            let db = Database::init(&app_data_dir).expect("Failed to initialize reminder database");

            // Initialize notification window state
            let notification_window_state = NotificationWindowState::new();

            // Store database and notification state in app state
            app.manage(AppState {
                db: db.clone(),
                notification_window: notification_window_state.clone(),
            });

            // Create the notification window using plain HTML/JS (no Blazor)
            // This avoids OPFS/SQLite conflicts with the main window
            let _notification_window = WebviewWindowBuilder::new(
                app,
                "notification-window",
                WebviewUrl::App("/notification-window.html".into()),
            )
            .title("")
            .inner_size(380.0, 280.0)
            .decorations(false)
            .always_on_top(true)
            .visible(false)
            .resizable(false)
            .skip_taskbar(true)
            .focused(false)
            .transparent(true)
            .build()
            .expect("Failed to create notification window");

            // Apply macOS-specific transparency and rounded corners
            apply_macos_transparency(&_notification_window);

            // Explicitly hide to override any saved state from window_state plugin
            let _ = _notification_window.hide();

            println!("Notification window created");

            // Start the background reminder monitor
            let app_handle = app.handle().clone();
            tauri::async_runtime::spawn(async move {
                reminder_monitor::start_monitor(app_handle, db, notification_window_state).await;
            });

            println!("Reminder service started");
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            greet,
            exchange_oauth_code,
            refresh_oauth_token,
            sync_event,
            delete_event,
            check_notification_permission,
            request_notification_permission,
            send_test_notification,
            // Persistent notification window commands
            show_notification_window,
            hide_notification_window,
            add_persistent_notification,
            dismiss_persistent_notification,
            get_persistent_notifications,
            get_notification_preference,
            set_notification_preference
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
