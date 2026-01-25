use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum CalendarType {
    Local,
    Microsoft,
    Google,
}

impl Default for CalendarType {
    fn default() -> Self {
        CalendarType::Local
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Calendar {
    pub id: String,
    pub name: String,
    pub display_name: Option<String>,
    pub color: String,
    pub calendar_type: CalendarType,
    pub external_account_id: Option<String>,
    pub external_id: Option<String>,
    pub is_hidden: bool,
    pub is_deleted: bool,
    pub enable_blocking: bool,
    pub receive_blocks: bool,
    pub created_at: String,
    pub updated_at: String,
}

impl Calendar {
    pub fn new(name: String, color: String) -> Self {
        let now = chrono::Utc::now().to_rfc3339();
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            name,
            display_name: None,
            color,
            calendar_type: CalendarType::Local,
            external_account_id: None,
            external_id: None,
            is_hidden: false,
            is_deleted: false,
            enable_blocking: false,
            receive_blocks: false,
            created_at: now.clone(),
            updated_at: now,
        }
    }

    /// Returns the name to display (display_name if set, otherwise name)
    pub fn get_display_name(&self) -> &str {
        self.display_name.as_deref().unwrap_or(&self.name)
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct NewCalendar {
    pub name: String,
    pub color: String,
    pub display_name: Option<String>,
}
