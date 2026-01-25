use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum ProviderType {
    Microsoft,
    Google,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum AccountStatus {
    Active,
    TokenExpired,
    Error,
    Disconnected,
}

impl Default for AccountStatus {
    fn default() -> Self {
        AccountStatus::Active
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ConnectedAccount {
    pub id: String,
    pub email: String,
    pub display_name: Option<String>,
    pub provider_type: ProviderType,
    pub access_token: String,
    pub refresh_token: Option<String>,
    pub token_expires_at: Option<String>,  // ISO 8601 datetime
    pub status: AccountStatus,
    pub last_sync_at: Option<String>,      // ISO 8601 datetime
    pub created_at: String,
    pub updated_at: String,
}

impl ConnectedAccount {
    pub fn new(
        email: String,
        provider_type: ProviderType,
        access_token: String,
        refresh_token: Option<String>,
        token_expires_at: Option<String>,
    ) -> Self {
        let now = chrono::Utc::now().to_rfc3339();
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            email,
            display_name: None,
            provider_type,
            access_token,
            refresh_token,
            token_expires_at,
            status: AccountStatus::Active,
            last_sync_at: None,
            created_at: now.clone(),
            updated_at: now,
        }
    }

    /// Check if the access token has expired
    pub fn is_token_expired(&self) -> bool {
        if let Some(ref expires_at) = self.token_expires_at {
            if let Ok(expiry) = chrono::DateTime::parse_from_rfc3339(expires_at) {
                return expiry < chrono::Utc::now();
            }
        }
        false
    }
}
