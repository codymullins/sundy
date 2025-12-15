using Mediator;
using Serilog;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for connecting to Microsoft Outlook via Graph API.
/// </summary>
public class ConnectOutlookCommandHandler(OutlookCalendarProvider outlookProvider, CalendarStore calendarStore)
    : ICommandHandler<ConnectOutlookCommand, ConnectOutlookResult>
{
    public async ValueTask<ConnectOutlookResult> Handle(ConnectOutlookCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Starting Outlook connection flow...");

            // Attempt to authenticate
            var success = await outlookProvider.ConnectAsync(cancellationToken);

            if (!success)
            {
                return new ConnectOutlookResult(false, ErrorMessage: "Authentication failed or was cancelled");
            }

            // Get the user's calendars from Outlook
            var outlookCalendars = await outlookProvider.GetCalendarsAsync(cancellationToken);

            if (outlookCalendars.Count == 0)
            {
                return new ConnectOutlookResult(true, outlookProvider.UserDisplayName, 
                    ErrorMessage: "No calendars found in Outlook");
            }

            // Create local calendar entries for each Outlook calendar
            foreach (var outlookCal in outlookCalendars)
            {
                // Check if we already have this calendar linked
                var existingCalendars = await calendarStore.GetAllAsync(cancellationToken);
                var existing = existingCalendars.FirstOrDefault(c => 
                    c.Type == CalendarType.Microsoft && c.Id == $"outlook_{outlookCal.Id}");

                if (existing == null)
                {
                    var calendar = new Calendar
                    {
                        Id = $"outlook_{outlookCal.Id}",
                        Name = $"{outlookCal.Name} (Outlook)",
                        Color = outlookCal.Color,
                        Type = CalendarType.Microsoft,
                        EnableBlocking = true,
                        ReceiveBlocks = false // Don't push blocks to Outlook by default
                    };

                    await calendarStore.AddAsync(calendar, cancellationToken);
                    Log.Information("Added Outlook calendar: {CalendarName}", calendar.Name);
                }
            }

            Log.Information("Successfully connected to Outlook as {UserName}, found {CalendarCount} calendars",
                outlookProvider.UserDisplayName, outlookCalendars.Count);

            return new ConnectOutlookResult(true, outlookProvider.UserDisplayName, Calendars: outlookCalendars);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to connect to Outlook");
            return new ConnectOutlookResult(false, ErrorMessage: ex.Message);
        }
    }
}

