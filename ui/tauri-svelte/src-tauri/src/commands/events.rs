use crate::db::DbState;
use crate::models::{CalendarEvent, NewEvent};
use tauri::State;

/// Get events within a date range, optionally filtered by calendar IDs
#[tauri::command]
pub fn get_events_in_range(
    start: String,
    end: String,
    calendar_ids: Option<Vec<String>>,
    db: State<DbState>,
) -> Result<Vec<CalendarEvent>, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let query = if let Some(ref ids) = calendar_ids {
        if ids.is_empty() {
            return Ok(vec![]);
        }
        let placeholders: Vec<String> = ids.iter().enumerate().map(|(i, _)| format!("?{}", i + 3)).collect();
        format!(
            "SELECT id, calendar_id, title, description, location, start_time, end_time, 
                    is_all_day, external_id, is_deleted, created_at, updated_at 
             FROM events 
             WHERE is_deleted = 0 
               AND start_time < ?2 
               AND end_time > ?1
               AND calendar_id IN ({})
             ORDER BY start_time",
            placeholders.join(", ")
        )
    } else {
        "SELECT id, calendar_id, title, description, location, start_time, end_time, 
                is_all_day, external_id, is_deleted, created_at, updated_at 
         FROM events 
         WHERE is_deleted = 0 
           AND start_time < ?2 
           AND end_time > ?1
         ORDER BY start_time".to_string()
    };

    let mut stmt = conn
        .prepare(&query)
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    let events = if let Some(ids) = calendar_ids {
        let mut params: Vec<Box<dyn rusqlite::ToSql>> = vec![
            Box::new(start),
            Box::new(end),
        ];
        for id in ids {
            params.push(Box::new(id));
        }
        let params_refs: Vec<&dyn rusqlite::ToSql> = params.iter().map(|p| p.as_ref()).collect();
        
        stmt.query_map(params_refs.as_slice(), |row| {
            Ok(CalendarEvent {
                id: row.get(0)?,
                calendar_id: row.get(1)?,
                title: row.get(2)?,
                description: row.get(3)?,
                location: row.get(4)?,
                start_time: row.get(5)?,
                end_time: row.get(6)?,
                is_all_day: row.get::<_, i32>(7)? != 0,
                external_id: row.get(8)?,
                is_deleted: row.get::<_, i32>(9)? != 0,
                created_at: row.get(10)?,
                updated_at: row.get(11)?,
            })
        })
        .map_err(|e| format!("Failed to query events: {}", e))?
        .collect::<Result<Vec<_>, _>>()
        .map_err(|e| format!("Failed to collect events: {}", e))?
    } else {
        stmt.query_map([&start, &end], |row| {
            Ok(CalendarEvent {
                id: row.get(0)?,
                calendar_id: row.get(1)?,
                title: row.get(2)?,
                description: row.get(3)?,
                location: row.get(4)?,
                start_time: row.get(5)?,
                end_time: row.get(6)?,
                is_all_day: row.get::<_, i32>(7)? != 0,
                external_id: row.get(8)?,
                is_deleted: row.get::<_, i32>(9)? != 0,
                created_at: row.get(10)?,
                updated_at: row.get(11)?,
            })
        })
        .map_err(|e| format!("Failed to query events: {}", e))?
        .collect::<Result<Vec<_>, _>>()
        .map_err(|e| format!("Failed to collect events: {}", e))?
    };

    Ok(events)
}

/// Get an event by ID
#[tauri::command]
pub fn get_event_by_id(id: String, db: State<DbState>) -> Result<Option<CalendarEvent>, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let result = conn.query_row(
        "SELECT id, calendar_id, title, description, location, start_time, end_time, 
                is_all_day, external_id, is_deleted, created_at, updated_at 
         FROM events 
         WHERE id = ?1 AND is_deleted = 0",
        [&id],
        |row| {
            Ok(CalendarEvent {
                id: row.get(0)?,
                calendar_id: row.get(1)?,
                title: row.get(2)?,
                description: row.get(3)?,
                location: row.get(4)?,
                start_time: row.get(5)?,
                end_time: row.get(6)?,
                is_all_day: row.get::<_, i32>(7)? != 0,
                external_id: row.get(8)?,
                is_deleted: row.get::<_, i32>(9)? != 0,
                created_at: row.get(10)?,
                updated_at: row.get(11)?,
            })
        },
    );

    match result {
        Ok(event) => Ok(Some(event)),
        Err(rusqlite::Error::QueryReturnedNoRows) => Ok(None),
        Err(e) => Err(format!("Failed to get event: {}", e)),
    }
}

/// Create a new event
#[tauri::command]
pub fn create_event(event: NewEvent, db: State<DbState>) -> Result<CalendarEvent, String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();
    let id = uuid::Uuid::new_v4().to_string();

    conn.execute(
        "INSERT INTO events (id, calendar_id, title, description, location, start_time, end_time, is_all_day, created_at, updated_at) 
         VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10)",
        [
            &id,
            &event.calendar_id,
            &event.title,
            &event.description.clone().unwrap_or_default(),
            &event.location.clone().unwrap_or_default(),
            &event.start_time,
            &event.end_time,
            &(event.is_all_day as i32).to_string(),
            &now,
            &now,
        ],
    )
    .map_err(|e| format!("Failed to create event: {}", e))?;

    Ok(CalendarEvent {
        id,
        calendar_id: event.calendar_id,
        title: event.title,
        description: event.description,
        location: event.location,
        start_time: event.start_time,
        end_time: event.end_time,
        is_all_day: event.is_all_day,
        external_id: None,
        is_deleted: false,
        created_at: now.clone(),
        updated_at: now,
    })
}

/// Update an existing event
#[tauri::command]
pub fn update_event(event: CalendarEvent, db: State<DbState>) -> Result<(), String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();

    conn.execute(
        "UPDATE events SET 
            calendar_id = ?1, title = ?2, description = ?3, location = ?4,
            start_time = ?5, end_time = ?6, is_all_day = ?7, updated_at = ?8
         WHERE id = ?9",
        [
            &event.calendar_id,
            &event.title,
            &event.description.unwrap_or_default(),
            &event.location.unwrap_or_default(),
            &event.start_time,
            &event.end_time,
            &(event.is_all_day as i32).to_string(),
            &now,
            &event.id,
        ],
    )
    .map_err(|e| format!("Failed to update event: {}", e))?;

    Ok(())
}

/// Delete an event (soft delete)
#[tauri::command]
pub fn delete_event(id: String, db: State<DbState>) -> Result<(), String> {
    let conn = db.conn.lock().map_err(|e| e.to_string())?;

    let now = chrono::Utc::now().to_rfc3339();

    conn.execute(
        "UPDATE events SET is_deleted = 1, updated_at = ?1 WHERE id = ?2",
        [&now, &id],
    )
    .map_err(|e| format!("Failed to delete event: {}", e))?;

    Ok(())
}
