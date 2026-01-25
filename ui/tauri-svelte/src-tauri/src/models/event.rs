use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct CalendarEvent {
    pub id: String,
    pub calendar_id: String,
    pub title: String,
    pub description: Option<String>,
    pub location: Option<String>,
    pub start_time: String,  // ISO 8601 datetime
    pub end_time: String,    // ISO 8601 datetime
    pub is_all_day: bool,
    pub external_id: Option<String>,
    pub is_deleted: bool,
    pub created_at: String,
    pub updated_at: String,
}

impl CalendarEvent {
    pub fn new(calendar_id: String, title: String, start_time: String, end_time: String) -> Self {
        let now = chrono::Utc::now().to_rfc3339();
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            calendar_id,
            title,
            description: None,
            location: None,
            start_time,
            end_time,
            is_all_day: false,
            external_id: None,
            is_deleted: false,
            created_at: now.clone(),
            updated_at: now,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct NewEvent {
    pub calendar_id: String,
    pub title: String,
    pub description: Option<String>,
    pub location: Option<String>,
    pub start_time: String,
    pub end_time: String,
    pub is_all_day: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct UpdateEvent {
    pub id: String,
    pub calendar_id: Option<String>,
    pub title: Option<String>,
    pub description: Option<String>,
    pub location: Option<String>,
    pub start_time: Option<String>,
    pub end_time: Option<String>,
    pub is_all_day: Option<bool>,
}
