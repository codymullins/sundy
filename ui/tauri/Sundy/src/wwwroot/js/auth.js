// OAuth authentication helpers - supports both Tauri and browser-only modes

// Platform detection
function isTauri() {
    return typeof window.__TAURI__ !== 'undefined';
}

// Store for pending auth promises
let authResolve = null;
let authReject = null;
let authWindow = null;
let authState = null;

// Initialize event listeners based on platform
(async function() {
    if (isTauri()) {
        // Tauri mode: listen for oauth-callback event
        try {
            const { listen } = window.__TAURI__.event;
            await listen('oauth-callback', (event) => {
                console.log('Received oauth-callback event:', event.payload);
                handleAuthCallback(event.payload);
            });
        } catch (e) {
            console.log('Tauri event listener setup:', e);
        }
    } else {
        // Browser mode: listen for postMessage from popup
        window.addEventListener('message', (event) => {
            // Verify origin matches our app
            if (event.origin !== window.location.origin) {
                return;
            }
            if (event.data && event.data.type === 'oauth-callback') {
                console.log('Received postMessage oauth-callback:', event.data);
                handleAuthCallback(event.data);
            }
        });
    }
})();

function handleAuthCallback(data) {
    console.log('handleAuthCallback:', data);
    if (authResolve) {
        // Verify state matches to prevent CSRF
        if (authState && data.state !== authState) {
            if (authReject) {
                authReject('State mismatch - possible CSRF attack');
            }
        } else {
            authResolve({
                code: data.code,
                state: data.state
            });
        }
        authResolve = null;
        authReject = null;
        authState = null;
    }

    // Close the auth window
    closeAuthWindow();
}

async function closeAuthWindow() {
    if (authWindow) {
        try {
            if (isTauri() && authWindow.close) {
                await authWindow.close();
            } else if (authWindow.closed === false) {
                authWindow.close();
            }
        } catch (e) {
            console.log('Window close error (may already be closed):', e);
        }
        authWindow = null;
    }
}

// Called from Blazor to open auth popup
window.openAuthPopup = async function(url, state) {
    console.log('openAuthPopup called with URL:', url, 'isTauri:', isTauri());

    return new Promise(async (resolve, reject) => {
        authResolve = resolve;
        authReject = reject;
        authState = state;

        try {
            // Close any existing auth window
            await closeAuthWindow();

            if (isTauri()) {
                // Tauri mode: use WebviewWindow
                const { WebviewWindow } = window.__TAURI__.webviewWindow;

                authWindow = new WebviewWindow('oauth-login', {
                    url: url,
                    title: 'Sign in with Microsoft',
                    width: 500,
                    height: 700,
                    center: true,
                    resizable: true,
                    decorations: true,
                    focus: true
                });

                console.log('Created WebviewWindow for OAuth');

                authWindow.once('tauri://created', () => {
                    console.log('OAuth window created successfully');
                });

                authWindow.once('tauri://destroyed', () => {
                    console.log('OAuth window destroyed');
                    if (authResolve) {
                        reject('Authentication cancelled');
                        authResolve = null;
                        authReject = null;
                        authState = null;
                    }
                    authWindow = null;
                });

                authWindow.once('tauri://error', (e) => {
                    console.error('OAuth window error:', e);
                    reject('Failed to open auth window: ' + JSON.stringify(e));
                    authResolve = null;
                    authReject = null;
                    authState = null;
                    authWindow = null;
                });
            } else {
                // Browser mode: use window.open popup
                const width = 500;
                const height = 700;
                const left = (window.screen.width - width) / 2;
                const top = (window.screen.height - height) / 2;

                authWindow = window.open(
                    url,
                    'oauth-login',
                    `width=${width},height=${height},left=${left},top=${top},popup=yes,toolbar=no,menubar=no`
                );

                if (!authWindow) {
                    reject('Popup blocked. Please allow popups for this site.');
                    authResolve = null;
                    authReject = null;
                    authState = null;
                    return;
                }

                console.log('Created browser popup for OAuth');

                // Poll to detect if popup was closed without completing auth
                const pollTimer = setInterval(() => {
                    if (authWindow && authWindow.closed) {
                        clearInterval(pollTimer);
                        if (authResolve) {
                            reject('Authentication cancelled');
                            authResolve = null;
                            authReject = null;
                            authState = null;
                        }
                        authWindow = null;
                    }
                }, 500);
            }
        } catch (e) {
            console.error('Failed to create OAuth window:', e);
            reject('Failed to open auth window: ' + e.message);
            authResolve = null;
            authReject = null;
            authState = null;
        }
    });
};

// Exchange authorization code for tokens (browser mode only)
// In Tauri mode, this is handled by Rust backend
window.exchangeOAuthCode = async function(code, codeVerifier, redirectUri, clientId) {
    console.log('exchangeOAuthCode called, code length:', code?.length);

    const tokenEndpoint = 'https://login.microsoftonline.com/common/oauth2/v2.0/token';

    const params = new URLSearchParams();
    params.append('client_id', clientId);
    params.append('grant_type', 'authorization_code');
    params.append('code', code);
    params.append('redirect_uri', redirectUri);
    params.append('code_verifier', codeVerifier);

    const response = await fetch(tokenEndpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: params.toString()
    });

    if (!response.ok) {
        const errorText = await response.text();
        console.error('Token exchange failed:', errorText);
        throw new Error('Token exchange failed: ' + response.status);
    }

    const tokenData = await response.json();
    return {
        access_token: tokenData.access_token,
        refresh_token: tokenData.refresh_token,
        expires_in: tokenData.expires_in,
        token_type: tokenData.token_type
    };
};

// Refresh OAuth token (browser mode only)
window.refreshOAuthToken = async function(refreshToken, clientId) {
    console.log('refreshOAuthToken called');

    const tokenEndpoint = 'https://login.microsoftonline.com/common/oauth2/v2.0/token';
    const scope = 'user.read Calendars.ReadWrite offline_access';

    const params = new URLSearchParams();
    params.append('client_id', clientId);
    params.append('grant_type', 'refresh_token');
    params.append('refresh_token', refreshToken);
    params.append('scope', scope);

    const response = await fetch(tokenEndpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: params.toString()
    });

    if (!response.ok) {
        const errorText = await response.text();
        console.error('Token refresh failed:', errorText);
        throw new Error('Token refresh failed: ' + response.status);
    }

    const tokenData = await response.json();
    return {
        access_token: tokenData.access_token,
        refresh_token: tokenData.refresh_token,
        expires_in: tokenData.expires_in,
        token_type: tokenData.token_type
    };
};

// Check if running in Tauri (exposed to Blazor)
window.isTauriEnvironment = function() {
    return isTauri();
};
