using Mediator;
using Microsoft.Extensions.Logging;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for getting events from Outlook calendars.
/// Supports multiple accounts via IMicrosoftAccountManager.
/// </summary>
public class GetOutlookEventsQueryHandler(
    OutlookCalendarProvider outlookProvider,
    ICalendarStore calendarStore,
    IMicrosoftAccountManager accountManager,
    ILogger<GetOutlookEventsQueryHandler> logger)
    : IQueryHandler<GetOutlookEventsQuery, List<CalendarEvent>>
{
    public async ValueTask<List<CalendarEvent>> Handle(GetOutlookEventsQuery query, CancellationToken cancellationToken)
    {
        var allEvents = new List<CalendarEvent>();

        try
        {
            // Get all Outlook calendars we have registered
            var calendars = await calendarStore.GetAllAsync(cancellationToken);
            var outlookCalendars = calendars.Where(c => c.Type == CalendarType.Microsoft).ToList();

            foreach (var calendar in outlookCalendars)
            {
                // Need ExternalAccountId to fetch from the right account
                if (string.IsNullOrEmpty(calendar.ExternalAccountId))
                {
                    continue;
                }

                // Check if the account is still authenticated
                if (!accountManager.IsAccountAuthenticated(calendar.ExternalAccountId))
                {
                    continue;
                }

                // Extract the actual Outlook calendar ID from our ID format
                var graphCalendarId = calendar.Id.Replace("outlook_", "");

                // Filter by specific calendar if requested
                if (query.CalendarId != null && calendar.Id != query.CalendarId)
                {
                    continue;
                }

                try
                {
                    var events = await outlookProvider.GetEventsAsync(
                        calendar.ExternalAccountId,
                        graphCalendarId,
                        query.Start,
                        query.End,
                        cancellationToken);

                    // Update CalendarId to use our internal ID format
                    foreach (var evt in events)
                    {
                        evt.CalendarId = calendar.Id;
                    }

                    allEvents.AddRange(events);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to fetch events from Outlook calendar {CalendarId}", calendar.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get Outlook events");
        }

        return allEvents;
    }
}

