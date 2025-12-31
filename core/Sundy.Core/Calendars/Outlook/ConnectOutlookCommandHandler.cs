using Mediator;
using Microsoft.Extensions.Logging;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for connecting to Microsoft Outlook via Graph API.
/// Supports multiple accounts via IMicrosoftAccountManager.
/// </summary>
public class ConnectOutlookCommandHandler(
    IMicrosoftAccountManager accountManager,
    OutlookCalendarProvider outlookProvider,
    ICalendarStore calendarStore,
    ILogger<ConnectOutlookCommandHandler> logger)
    : ICommandHandler<ConnectOutlookCommand, ConnectOutlookResult>
{
    public async ValueTask<ConnectOutlookResult> Handle(ConnectOutlookCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Add a new account via the account manager (triggers auth flow)
            var account = await accountManager.AddAccountAsync(cancellationToken);

            // Get the user's calendars from Outlook for this account
            var outlookCalendars = await outlookProvider.GetCalendarsAsync(account.Id, cancellationToken);

            if (outlookCalendars.Count == 0)
            {
                return new ConnectOutlookResult(true, account,
                    ErrorMessage: "No calendars found in Outlook");
            }

            // Create local calendar entries for each Outlook calendar
            foreach (var outlookCal in outlookCalendars)
            {
                // Check if we already have this calendar linked
                var existingCalendars = await calendarStore.GetAllAsync(cancellationToken);
                var existing = existingCalendars.FirstOrDefault(c =>
                    c.Type == CalendarType.Microsoft &&
                    c.Id == $"outlook_{outlookCal.Id}" &&
                    c.ExternalAccountId == account.Id);

                if (existing == null)
                {
                    // Auto-hide system calendars (Birthdays, Holidays)
                    var isSystemCalendar = outlookCal.Name.Contains("Birthdays", StringComparison.OrdinalIgnoreCase)
                        || outlookCal.Name.Contains("holidays", StringComparison.OrdinalIgnoreCase);

                    var calendar = new Calendar
                    {
                        Id = $"outlook_{outlookCal.Id}",
                        Name = $"{outlookCal.Name} ({account.Email})",
                        Color = outlookCal.Color,
                        Type = CalendarType.Microsoft,
                        EnableBlocking = true,
                        ReceiveBlocks = false, // Don't push blocks to Outlook by default
                        ExternalAccountId = account.Id,
                        IsHidden = isSystemCalendar
                    };

                    await calendarStore.AddAsync(calendar, cancellationToken);
                }
            }

            return new ConnectOutlookResult(true, account, Calendars: outlookCalendars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect Outlook account");
            return new ConnectOutlookResult(false, ErrorMessage: ex.Message);
        }
    }
}

