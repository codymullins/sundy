use rusqlite::{Connection, Result, params};
use std::path::Path;
use std::sync::{Arc, Mutex};

/// Event data for reminder scheduling
#[derive(Debug, Clone)]
pub struct ReminderEvent {
    pub id: String,
    pub title: String,
    pub start_time: i64,  // Unix timestamp
    pub reminder_minutes: i32,
}

/// Thread-safe database wrapper
pub struct Database {
    conn: Arc<Mutex<Connection>>,
}

impl Database {
    /// Get a reference to the connection for direct queries
    pub fn get_connection(&self) -> Arc<Mutex<Connection>> {
        Arc::clone(&self.conn)
    }

    /// Initialize the database at the given path
    pub fn init(app_data_dir: &Path) -> Result<Self> {
        std::fs::create_dir_all(app_data_dir).ok();
        let db_path = app_data_dir.join("reminders.db");
        let conn = Connection::open(db_path)?;

        // Create events table for reminder tracking
        conn.execute(
            "CREATE TABLE IF NOT EXISTS events (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                start_time INTEGER NOT NULL,
                reminder_minutes INTEGER DEFAULT 15,
                reminder_sent INTEGER DEFAULT 0
            )",
            [],
        )?;

        // Create index for efficient querying of upcoming events
        conn.execute(
            "CREATE INDEX IF NOT EXISTS idx_events_start_time ON events (start_time)",
            [],
        )?;

        Ok(Self {
            conn: Arc::new(Mutex::new(conn)),
        })
    }

    /// Insert or update an event
    /// Always resets reminder_sent on any update so edited events get re-notified
    pub fn upsert_event(&self, event: &ReminderEvent) -> Result<()> {
        let conn = self.conn.lock().unwrap();
        conn.execute(
            "INSERT INTO events (id, title, start_time, reminder_minutes, reminder_sent)
             VALUES (?1, ?2, ?3, ?4, 0)
             ON CONFLICT(id) DO UPDATE SET
                title = excluded.title,
                start_time = excluded.start_time,
                reminder_minutes = excluded.reminder_minutes,
                reminder_sent = 0",
            params![event.id, event.title, event.start_time, event.reminder_minutes],
        )?;
        Ok(())
    }

    /// Delete an event by ID
    pub fn delete_event(&self, id: &str) -> Result<()> {
        let conn = self.conn.lock().unwrap();
        conn.execute("DELETE FROM events WHERE id = ?1", params![id])?;
        Ok(())
    }

    /// Get events that have reminders due (reminder time has passed, not yet sent)
    pub fn get_due_reminders(&self, now_timestamp: i64) -> Result<Vec<ReminderEvent>> {
        let conn = self.conn.lock().unwrap();
        let mut stmt = conn.prepare(
            "SELECT id, title, start_time, reminder_minutes
             FROM events
             WHERE reminder_sent = 0
               AND (start_time - (reminder_minutes * 60)) <= ?1
               AND start_time > (?1 - 300)
             ORDER BY start_time ASC",
        )?;

        let events = stmt
            .query_map(params![now_timestamp], |row| {
                Ok(ReminderEvent {
                    id: row.get(0)?,
                    title: row.get(1)?,
                    start_time: row.get(2)?,
                    reminder_minutes: row.get(3)?,
                })
            })?
            .collect::<Result<Vec<_>>>()?;

        Ok(events)
    }

    /// Mark a reminder as sent
    pub fn mark_reminder_sent(&self, id: &str) -> Result<()> {
        let conn = self.conn.lock().unwrap();
        conn.execute(
            "UPDATE events SET reminder_sent = 1 WHERE id = ?1",
            params![id],
        )?;
        Ok(())
    }

    /// Clean up old events (past events older than 24 hours)
    pub fn cleanup_old_events(&self, now_timestamp: i64) -> Result<usize> {
        let conn = self.conn.lock().unwrap();
        let cutoff = now_timestamp - (24 * 60 * 60); // 24 hours ago
        let deleted = conn.execute(
            "DELETE FROM events WHERE start_time < ?1",
            params![cutoff],
        )?;
        Ok(deleted)
    }
}

impl Clone for Database {
    fn clone(&self) -> Self {
        Self {
            conn: Arc::clone(&self.conn),
        }
    }
}
