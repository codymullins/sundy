use rusqlite::Connection;
use super::schema::*;

/// Current database schema version
const SCHEMA_VERSION: i32 = 1;

/// Run all pending migrations
pub fn run_migrations(conn: &Connection) -> Result<(), String> {
    // Create the migrations table if it doesn't exist
    conn.execute(
        "CREATE TABLE IF NOT EXISTS schema_migrations (
            version INTEGER PRIMARY KEY,
            applied_at TEXT NOT NULL
        )",
        [],
    )
    .map_err(|e| format!("Failed to create migrations table: {}", e))?;

    // Get current version
    let current_version: i32 = conn
        .query_row(
            "SELECT COALESCE(MAX(version), 0) FROM schema_migrations",
            [],
            |row| row.get(0),
        )
        .unwrap_or(0);

    // Run migrations
    if current_version < 1 {
        migrate_v1(conn)?;
    }

    Ok(())
}

/// Migration v1: Initial schema
fn migrate_v1(conn: &Connection) -> Result<(), String> {
    // Create tables in correct order (accounts first due to foreign keys)
    conn.execute(CREATE_CONNECTED_ACCOUNTS_TABLE, [])
        .map_err(|e| format!("Failed to create connected_accounts table: {}", e))?;

    conn.execute(CREATE_CALENDARS_TABLE, [])
        .map_err(|e| format!("Failed to create calendars table: {}", e))?;

    conn.execute(CREATE_EVENTS_TABLE, [])
        .map_err(|e| format!("Failed to create events table: {}", e))?;

    conn.execute(CREATE_SETTINGS_TABLE, [])
        .map_err(|e| format!("Failed to create settings table: {}", e))?;

    conn.execute(CREATE_SYNC_METADATA_TABLE, [])
        .map_err(|e| format!("Failed to create sync_metadata table: {}", e))?;

    // Create indexes
    conn.execute(CREATE_EVENTS_CALENDAR_INDEX, [])
        .map_err(|e| format!("Failed to create events calendar index: {}", e))?;

    conn.execute(CREATE_EVENTS_TIME_INDEX, [])
        .map_err(|e| format!("Failed to create events time index: {}", e))?;

    conn.execute(CREATE_CALENDARS_ACCOUNT_INDEX, [])
        .map_err(|e| format!("Failed to create calendars account index: {}", e))?;

    // Create default local calendar
    let now = chrono::Utc::now().to_rfc3339();
    let default_id = uuid::Uuid::new_v4().to_string();
    conn.execute(
        "INSERT INTO calendars (id, name, color, calendar_type, created_at, updated_at) VALUES (?1, ?2, ?3, ?4, ?5, ?6)",
        [&default_id, "Personal", "#3b82f6", "Local", &now, &now],
    )
    .map_err(|e| format!("Failed to create default calendar: {}", e))?;

    // Insert default settings
    let settings = [
        ("syncIntervalMinutes", "15"),
        ("privacyMode", "false"),
        ("privacyHideEmails", "false"),
        ("privacyHideEventTitles", "false"),
        ("collapsePastEvents", "false"),
        ("dynamicViewEnabled", "true"),
    ];

    for (key, value) in settings {
        conn.execute(
            "INSERT OR IGNORE INTO settings (key, value, updated_at) VALUES (?1, ?2, ?3)",
            [key, value, &now],
        )
        .map_err(|e| format!("Failed to insert setting {}: {}", key, e))?;
    }

    // Record migration
    conn.execute(
        "INSERT INTO schema_migrations (version, applied_at) VALUES (?1, ?2)",
        [&SCHEMA_VERSION.to_string(), &now],
    )
    .map_err(|e| format!("Failed to record migration: {}", e))?;

    Ok(())
}
