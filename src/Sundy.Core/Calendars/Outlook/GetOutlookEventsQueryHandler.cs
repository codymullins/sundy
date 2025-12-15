using Mediator;
using Serilog;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for getting events from Outlook calendars.
/// </summary>
public class GetOutlookEventsQueryHandler(OutlookCalendarProvider outlookProvider, CalendarStore calendarStore)
    : IQueryHandler<GetOutlookEventsQuery, List<CalendarEvent>>
{
    public async ValueTask<List<CalendarEvent>> Handle(GetOutlookEventsQuery query, CancellationToken cancellationToken)
    {
        if (!outlookProvider.IsConnected)
        {
            Log.Warning("Outlook not connected, returning empty event list");
            return [];
        }

        var allEvents = new List<CalendarEvent>();

        try
        {
            // Get all Outlook calendars we have registered
            var calendars = await calendarStore.GetAllAsync(cancellationToken);
            var outlookCalendars = calendars.Where(c => c.Type == CalendarType.Microsoft).ToList();

            foreach (var calendar in outlookCalendars)
            {
                // Extract the actual Outlook calendar ID from our ID format
                var outlookCalendarId = calendar.Id.Replace("outlook_", "");

                // Filter by specific calendar if requested
                if (query.CalendarId != null && calendar.Id != query.CalendarId)
                {
                    continue;
                }

                try
                {
                    var events = await outlookProvider.GetEventsAsync(
                        outlookCalendarId, 
                        query.Start, 
                        query.End, 
                        cancellationToken);

                    // Update CalendarId to use our internal ID format
                    foreach (var evt in events)
                    {
                        evt.CalendarId = calendar.Id;
                    }

                    allEvents.AddRange(events);
                    Log.Debug("Fetched {EventCount} events from Outlook calendar {CalendarName}", 
                        events.Count, calendar.Name);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to fetch events from Outlook calendar {CalendarId}", calendar.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get Outlook events");
        }

        return allEvents;
    }
}

