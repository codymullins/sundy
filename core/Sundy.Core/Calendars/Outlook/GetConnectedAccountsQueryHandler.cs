using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for getting all connected Microsoft accounts.
/// </summary>
public class GetConnectedAccountsQueryHandler(IMicrosoftAccountManager accountManager)
    : IQueryHandler<GetConnectedAccountsQuery, List<ConnectedAccount>>
{
    public async ValueTask<List<ConnectedAccount>> Handle(GetConnectedAccountsQuery query, CancellationToken cancellationToken)
    {
        var accounts = await accountManager.GetConnectedAccountsAsync(cancellationToken);
        return accounts.ToList();
    }
}
