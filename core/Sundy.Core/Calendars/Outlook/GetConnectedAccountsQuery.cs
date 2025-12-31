using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Query to get all connected Microsoft accounts.
/// </summary>
public record GetConnectedAccountsQuery : IQuery<List<ConnectedAccount>>;
