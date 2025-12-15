using Azure.Core;
using Microsoft.Identity.Client;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// TokenCredential implementation that wraps MSAL PublicClientApplication for use with Microsoft Graph SDK.
/// </summary>
internal class MsalTokenCredential(IPublicClientApplication pca, string[] scopes) : TokenCredential
{
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var accounts = await pca.GetAccountsAsync();
        try
        {
            var result = await pca.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                .ExecuteAsync(cancellationToken);
            return new AccessToken(result.AccessToken, result.ExpiresOn);
        }
        catch (MsalUiRequiredException)
        {
            // Token expired or not in cache, but we can't do interactive auth here
            throw new InvalidOperationException("Token expired. Please re-authenticate.");
        }
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();
    }
}
