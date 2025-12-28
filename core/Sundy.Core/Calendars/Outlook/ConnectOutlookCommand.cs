using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Command to initiate connection to Microsoft Outlook via Graph API.
/// </summary>
public record ConnectOutlookCommand : ICommand<ConnectOutlookResult>;

/// <summary>
/// Result of connecting to Outlook.
/// </summary>
public record ConnectOutlookResult(
    bool Success, 
    string? UserDisplayName = null, 
    string? ErrorMessage = null,
    List<OutlookCalendarInfo>? Calendars = null);

