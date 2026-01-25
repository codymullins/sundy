/// SQL schema for the Sundycal database

pub const CREATE_CALENDARS_TABLE: &str = r#"
CREATE TABLE IF NOT EXISTS calendars (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    display_name TEXT,
    color TEXT NOT NULL DEFAULT '#3b82f6',
    calendar_type TEXT NOT NULL DEFAULT 'Local',
    external_account_id TEXT,
    external_id TEXT,
    is_hidden INTEGER NOT NULL DEFAULT 0,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    enable_blocking INTEGER NOT NULL DEFAULT 0,
    receive_blocks INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (external_account_id) REFERENCES connected_accounts(id) ON DELETE SET NULL
)
"#;

pub const CREATE_EVENTS_TABLE: &str = r#"
CREATE TABLE IF NOT EXISTS events (
    id TEXT PRIMARY KEY NOT NULL,
    calendar_id TEXT NOT NULL,
    title TEXT NOT NULL,
    description TEXT,
    location TEXT,
    start_time TEXT NOT NULL,
    end_time TEXT NOT NULL,
    is_all_day INTEGER NOT NULL DEFAULT 0,
    external_id TEXT,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (calendar_id) REFERENCES calendars(id) ON DELETE CASCADE
)
"#;

pub const CREATE_CONNECTED_ACCOUNTS_TABLE: &str = r#"
CREATE TABLE IF NOT EXISTS connected_accounts (
    id TEXT PRIMARY KEY NOT NULL,
    email TEXT NOT NULL,
    display_name TEXT,
    provider_type TEXT NOT NULL,
    access_token TEXT NOT NULL,
    refresh_token TEXT,
    token_expires_at TEXT,
    status TEXT NOT NULL DEFAULT 'Active',
    last_sync_at TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
)
"#;

pub const CREATE_SETTINGS_TABLE: &str = r#"
CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY NOT NULL,
    value TEXT NOT NULL,
    updated_at TEXT NOT NULL
)
"#;

pub const CREATE_SYNC_METADATA_TABLE: &str = r#"
CREATE TABLE IF NOT EXISTS sync_metadata (
    calendar_id TEXT PRIMARY KEY NOT NULL,
    delta_token TEXT,
    last_synced_at TEXT,
    sync_error TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (calendar_id) REFERENCES calendars(id) ON DELETE CASCADE
)
"#;

// Indexes for better query performance
pub const CREATE_EVENTS_CALENDAR_INDEX: &str = r#"
CREATE INDEX IF NOT EXISTS idx_events_calendar_id ON events(calendar_id)
"#;

pub const CREATE_EVENTS_TIME_INDEX: &str = r#"
CREATE INDEX IF NOT EXISTS idx_events_start_time ON events(start_time)
"#;

pub const CREATE_CALENDARS_ACCOUNT_INDEX: &str = r#"
CREATE INDEX IF NOT EXISTS idx_calendars_account_id ON calendars(external_account_id)
"#;
