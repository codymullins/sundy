use serde::{Deserialize, Serialize};

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

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            greet,
            exchange_oauth_code,
            refresh_oauth_token
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
