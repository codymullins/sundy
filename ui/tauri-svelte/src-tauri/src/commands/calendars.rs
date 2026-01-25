use crate::db::DbState;
use crate::models::{Calendar, CalendarType, NewCalendar};
use tauri::State;

/// Get all calendars (excluding soft-deleted ones)
#[tauri::command]
pub fn get_all_calendars(db: State<DbState>) -> Result<Vec<Calendar>, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let mut stmt = conn
        .prepare(
            "SELECT id, name, display_name, color, calendar_type, external_account_id, 
                    external_id, is_hidden, is_deleted, enable_blocking, receive_blocks,
                    created_at, updated_at 
             FROM calendars 
             WHERE is_deleted = 0
             ORDER BY name",
        )
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    let calendars = stmt
        .query_map([], |row| {
            Ok(Calendar {
                id: row.get(0)?,
                name: row.get(1)?,
                display_name: row.get(2)?,
                color: row.get(3)?,
                calendar_type: parse_calendar_type(row.get::<_, String>(4)?),
                external_account_id: row.get(5)?,
                external_id: row.get(6)?,
                is_hidden: row.get::<_, i32>(7)? != 0,
                is_deleted: row.get::<_, i32>(8)? != 0,
                enable_blocking: row.get::<_, i32>(9)? != 0,
                receive_blocks: row.get::<_, i32>(10)? != 0,
                created_at: row.get(11)?,
                updated_at: row.get(12)?,
            })
        })
        .map_err(|e| format!("Failed to query calendars: {}", e))?
        .collect::<Result<Vec<_>, _>>()
        .map_err(|e| format!("Failed to collect calendars: {}", e))?;

    Ok(calendars)
}

/// Get a calendar by ID
#[tauri::command]
pub fn get_calendar_by_id(id: String, db: State<DbState>) -> Result<Option<Calendar>, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let result = conn.query_row(
        "SELECT id, name, display_name, color, calendar_type, external_account_id, 
                external_id, is_hidden, is_deleted, enable_blocking, receive_blocks,
                created_at, updated_at 
         FROM calendars 
         WHERE id = ?1 AND is_deleted = 0",
        [&id],
        |row| {
            Ok(Calendar {
                id: row.get(0)?,
                name: row.get(1)?,
                display_name: row.get(2)?,
                color: row.get(3)?,
                calendar_type: parse_calendar_type(row.get::<_, String>(4)?),
                external_account_id: row.get(5)?,
                external_id: row.get(6)?,
                is_hidden: row.get::<_, i32>(7)? != 0,
                is_deleted: row.get::<_, i32>(8)? != 0,
                enable_blocking: row.get::<_, i32>(9)? != 0,
                receive_blocks: row.get::<_, i32>(10)? != 0,
                created_at: row.get(11)?,
                updated_at: row.get(12)?,
            })
        },
    );

    match result {
        Ok(calendar) => Ok(Some(calendar)),
        Err(rusqlite::Error::QueryReturnedNoRows) => Ok(None),
        Err(e) => Err(format!("Failed to get calendar: {}", e)),
    }
}

/// Create a new calendar
#[tauri::command]
pub fn create_calendar(calendar: NewCalendar, db: State<DbState>) -> Result<Calendar, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();
    let id = uuid::Uuid::new_v4().to_string();
    let display_name = calendar.display_name.clone();

    conn.execute(
        "INSERT INTO calendars (id, name, display_name, color, calendar_type, created_at, updated_at) 
         VALUES (?1, ?2, ?3, ?4, 'Local', ?5, ?6)",
        [
            &id,
            &calendar.name,
            &display_name.clone().unwrap_or_default(),
            &calendar.color,
            &now,
            &now,
        ],
    )
    .map_err(|e| format!("Failed to create calendar: {}", e))?;

    Ok(Calendar {
        id,
        name: calendar.name,
        display_name,
        color: calendar.color,
        calendar_type: CalendarType::Local,
        external_account_id: None,
        external_id: None,
        is_hidden: false,
        is_deleted: false,
        enable_blocking: false,
        receive_blocks: false,
        created_at: now.clone(),
        updated_at: now,
    })
}

/// Update an existing calendar
#[tauri::command]
pub fn update_calendar(calendar: Calendar, db: State<DbState>) -> Result<(), String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();

    conn.execute(
        "UPDATE calendars SET 
            name = ?1, display_name = ?2, color = ?3, is_hidden = ?4, 
            enable_blocking = ?5, receive_blocks = ?6, updated_at = ?7
         WHERE id = ?8",
        [
            &calendar.name,
            &calendar.display_name.clone().unwrap_or_default(),
            &calendar.color,
            &(calendar.is_hidden as i32).to_string(),
            &(calendar.enable_blocking as i32).to_string(),
            &(calendar.receive_blocks as i32).to_string(),
            &now,
            &calendar.id,
        ],
    )
    .map_err(|e| format!("Failed to update calendar: {}", e))?;

    Ok(())
}

/// Delete a calendar (soft delete)
#[tauri::command]
pub fn delete_calendar(id: String, db: State<DbState>) -> Result<(), String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();

    conn.execute(
        "UPDATE calendars SET is_deleted = 1, updated_at = ?1 WHERE id = ?2",
        [&now, &id],
    )
    .map_err(|e| format!("Failed to delete calendar: {}", e))?;

    // Also soft-delete all events in this calendar
    conn.execute(
        "UPDATE events SET is_deleted = 1, updated_at = ?1 WHERE calendar_id = ?2",
        [&now, &id],
    )
    .map_err(|e| format!("Failed to delete calendar events: {}", e))?;

    Ok(())
}

fn parse_calendar_type(s: String) -> CalendarType {
    match s.as_str() {
        "Microsoft" => CalendarType::Microsoft,
        "Google" => CalendarType::Google,
        _ => CalendarType::Local,
    }
}
