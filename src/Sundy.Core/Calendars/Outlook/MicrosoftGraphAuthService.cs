using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Service for authenticating with Microsoft Graph using device code flow.
/// This flow is suitable for desktop/console apps where a browser can be launched.
/// </summary>
public class MicrosoftGraphAuthService(ILogger<MicrosoftGraphAuthService> log, OutlookGraphOptions? options = null)
{
    private readonly OutlookGraphOptions _options = options ?? new OutlookGraphOptions();
    private GraphServiceClient? _graphClient;
    private string? _userDisplayName;
    private bool _isAuthenticated;

    public bool IsAuthenticated => _isAuthenticated;
    public string? UserDisplayName => _userDisplayName;

    public event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;
    private readonly string[] scopes = ["user.read", "Calendars.ReadWrite"];

    private async Task<AuthenticationResult> AcquireByDeviceCodeAsync(IPublicClientApplication pca)
    {
        try
        {
            var result = await pca.AcquireTokenWithDeviceCode(scopes,
                deviceCodeResult =>
                {
                    // This will print the message on the console which tells the user where to go sign-in using 
                    // a separate browser and the code to enter once they sign in.
                    // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                    // device code callback to look for the successful login of the user via that browser.
                    // This background polling (whose interval and timeout data is also provided as fields in the
                    // deviceCodeCallback class) will occur until:
                    // * The user has successfully logged in via browser and entered the proper code
                    // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                    // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                    //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).

                    // Parse and fire device code event for UI display
                    var deviceCodeInfo = ParseDeviceCodeMessage(deviceCodeResult);
                    DeviceCodeReceived?.Invoke(this, deviceCodeInfo);

                    return Task.FromResult(0);
                }).ExecuteAsync();

            return result;
        }
        // TODO: handle or throw all these exceptions
        catch (MsalServiceException ex)
        {
            // Kind of errors you could have (in ex.Message)

            // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
            // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted. you have probably created
            // your public client application with the following authorities:
            // https://login.microsoftonline.com/common or https://login.microsoftonline.com/organizations

            // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
            // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted

            // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are 
            // no active subscriptions for the tenant. Check with your subscription administrator.
            // Mitigation: if you have an active subscription for the tenant this might be that you have a typo in the 
            // tenantId (GUID) or tenant domain name.
            log.LogError(ex, "Device code authentication failed due to service error.");
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // If you use a CancellationToken, and call the Cancel() method on it, then this *may* be triggered
            // to indicate that the operation was cancelled. 
            // See /dotnet/standard/threading/cancellation-in-managed-threads 
            // for more detailed information on how C# supports cancellation in managed threads.
            log.LogError(ex, "Device code authentication was cancelled.");
            throw;
        }
        catch (MsalClientException ex)
        {
            // Possible cause - verification code expired before contacting the server
            // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
            // call to `AcquireTokenWithDeviceCode` is not cancelled in between
            log.LogError(ex, "Device code authentication failed due to client error.");
            throw;
        }

    }

    /// <summary>
    /// Parses the device code result to extract URL and code.
    /// </summary>
    private DeviceCodeInfo ParseDeviceCodeMessage(DeviceCodeResult deviceCodeResult)
    {
        // The message format is typically:
        // "To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code XXXXXXXX to authenticate."

        var message = deviceCodeResult.Message;
        var url = deviceCodeResult.VerificationUrl?.ToString() ?? "https://microsoft.com/devicelogin";
        var code = deviceCodeResult.UserCode;

        // Fallback parsing if properties aren't available
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(url))
        {
            try
            {
                // Extract URL using regex: https://... (before "and")
                var urlMatch = global::System.Text.RegularExpressions.Regex.Match(
                    message,
                    @"https://[^\s]+");
                if (urlMatch.Success)
                {
                    url = urlMatch.Value;
                }

                // Extract code: typically the last word before "to authenticate"
                var codeMatch = global::System.Text.RegularExpressions.Regex.Match(
                    message,
                    @"code\s+([A-Z0-9]+)");
                if (codeMatch.Success)
                {
                    code = codeMatch.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to parse device code message, using defaults");
            }
        }

        return new DeviceCodeInfo
        {
            VerificationUrl = url,
            UserCode = code ?? "UNKNOWN",
            FullMessage = message,
            ExpiresOn = deviceCodeResult.ExpiresOn
        };
    }

    public async Task<bool> AuthenticateWithDeviceCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AuthenticationResult? result;
            var pca = PublicClientApplicationBuilder.Create(_options.ClientId)
                .WithDefaultRedirectUri()
                .Build();

            var accounts = await pca.GetAccountsAsync();

            try
            {
                result = await pca.AcquireTokenSilent(
                        _options.GraphUserScopes ?? ["User.Read", "Calendars.ReadWrite"],
                        accounts.FirstOrDefault())
                    .ExecuteAsync(cancellationToken);
            }
            catch (MsalUiRequiredException)
            {
                // No token found in the cache or Azure AD insists that a form interactive auth is required (e.g. the tenant admin turned on MFA)
                // If you want to provide a more complex user experience, check out ex.Classification

                result = await AcquireByDeviceCodeAsync(pca);
            }

            // Create a custom TokenCredential that wraps the MSAL PublicClientApplication
            var tokenCredential = new MsalTokenCredential(pca, scopes);
            _graphClient = new GraphServiceClient(tokenCredential, scopes);

            // Test the connection by getting the current user
            var user = await _graphClient.Me.GetAsync(cancellationToken: cancellationToken);
            _userDisplayName = user?.DisplayName ?? user?.UserPrincipalName ?? result.Account.Username;
            _isAuthenticated = true;

            log.LogInformation("Successfully authenticated with device code as {UserName}", _userDisplayName);
            return true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to authenticate with device code");
            _isAuthenticated = false;
            _graphClient = null;
            return false;
        }
    }

    /// <summary>
    /// Initiates authentication using interactive browser-based flow.
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.ClientId))
            {
                log.LogError("Microsoft Graph Client ID is not configured");
                return false;
            }

            TokenCredential credential;
            string[] currentScopes;

            if (_options.UseDevelopmentCredential)
            {
                // For development credentials (VS, VS Code), we must use the .default scope
                // These credentials don't support multiple individual scopes
                log.LogInformation("Using development credentials (VS Code, Visual Studio, or Interactive Browser)");
                credential = new ChainedTokenCredential(
                    new VisualStudioCredential(new VisualStudioCredentialOptions
                    {
                        TenantId = _options.TenantId
                    }),
                    new VisualStudioCodeCredential(new VisualStudioCodeCredentialOptions
                    {
                        TenantId = _options.TenantId
                    }),
                    new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                    {
                        TenantId = _options.TenantId,
                        ClientId = _options.ClientId,
                        RedirectUri = new Uri("http://localhost")
                    })
                );

                // Use .default scope for development - requires pre-configured API permissions in Azure AD
                currentScopes = ["https://graph.microsoft.com/.default"];
            }
            else
            {
                // Use InteractiveBrowserCredential for production desktop apps
                // This will open the system browser for authentication
                credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                {
                    TenantId = _options.TenantId,
                    ClientId = _options.ClientId,
                    RedirectUri = new Uri("http://localhost")
                });
                // Use specific scopes for interactive login - user will consent to these
                currentScopes = _options.GraphUserScopes ?? scopes;
            }

            _graphClient = new GraphServiceClient(credential, currentScopes);

            // Log which scopes we're requesting for diagnostics
            log.LogInformation("Requesting Microsoft Graph scopes: {Scopes}", string.Join(", ", currentScopes));

            // Test the connection by getting the current user
            var user = await _graphClient.Me.GetAsync(cancellationToken: cancellationToken);
            _userDisplayName = user?.DisplayName ?? user?.UserPrincipalName ?? "Unknown User";
            _isAuthenticated = true;

            log.LogInformation("Successfully authenticated as {UserName}", _userDisplayName);
            return true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to authenticate with Microsoft Graph");
            _isAuthenticated = false;
            _graphClient = null;
            return false;
        }
    }
    
    public GraphServiceClient? GetClient() => _graphClient;

    /// <summary>
    /// Signs out and clears the authentication state.
    /// </summary>
    public void SignOut()
    {
        _graphClient = null;
        _isAuthenticated = false;
        _userDisplayName = null;
        log.LogInformation("Signed out of Microsoft Graph");
    }
}
