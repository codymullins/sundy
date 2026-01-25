pub mod schema;
pub mod migrations;

pub use schema::*;
pub use migrations::*;

use rusqlite::Connection;
use std::path::PathBuf;
use std::sync::Mutex;
use tauri::AppHandle;
use tauri::Manager;

/// Database state managed by Tauri
pub struct DbState {
    pub conn: Mutex<Connection>,
}

/// Get the database path for the application
pub fn get_db_path(app: &AppHandle) -> Result<PathBuf, String> {
    let app_data_dir = app
        .path()
        .app_data_dir()
        .map_err(|e| format!("Failed to get app data dir: {}", e))?;

    // Create the directory if it doesn't exist
    std::fs::create_dir_all(&app_data_dir)
        .map_err(|e| format!("Failed to create app data dir: {}", e))?;

    Ok(app_data_dir.join("sundy.db"))
}

/// Initialize the database connection and run migrations
pub fn initialize_database(app: &AppHandle) -> Result<DbState, String> {
    let db_path = get_db_path(app)?;
    
    let conn = Connection::open(&db_path)
        .map_err(|e| format!("Failed to open database: {}", e))?;

    // Enable foreign keys
    conn.execute_batch("PRAGMA foreign_keys = ON;")
        .map_err(|e| format!("Failed to enable foreign keys: {}", e))?;

    // Run migrations
    run_migrations(&conn)?;

    Ok(DbState {
        conn: Mutex::new(conn),
    })
}
