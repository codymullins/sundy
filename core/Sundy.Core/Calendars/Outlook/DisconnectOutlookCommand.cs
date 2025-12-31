using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Command to disconnect a Microsoft Outlook account.
/// </summary>
public record DisconnectOutlookCommand(string AccountId) : ICommand<DisconnectOutlookResult>;

/// <summary>
/// Result of disconnecting from Outlook.
/// </summary>
public record DisconnectOutlookResult(bool Success, string? ErrorMessage = null);
