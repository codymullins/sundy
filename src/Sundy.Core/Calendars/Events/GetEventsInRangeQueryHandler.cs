using Mediator;

using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetEventsInRangeQueryHandler(
    IEventStore repository,
    OutlookCalendarProvider outlookProvider,
    ICalendarStore calendarStore)
    : IRequestHandler<GetEventsInRangeQuery, List<CalendarEvent>>
{
    public async ValueTask<List<CalendarEvent>> Handle(GetEventsInRangeQuery request,
        CancellationToken cancellationToken)
    {
        // Get local events
        var localEvents = await repository.GetEventsInRangeAsync(
            request.StartTime,
            request.EndTime,
            request.CalendarId,
            request.VisibleCalendarIds,
            cancellationToken);

        // Get Outlook events if connected
        var allEvents = new List<CalendarEvent>(localEvents);

        if (outlookProvider.IsConnected)
        {
            try
            {
                // Get all Outlook calendars we have registered
                var calendars = await calendarStore.GetAllAsync(cancellationToken);
                var outlookCalendars = calendars.Where(c => c.Type == CalendarType.Microsoft).ToList();

                // If connected but no calendars synced yet, sync them now
                if (outlookCalendars.Count == 0)
                {
                    outlookCalendars = await SyncOutlookCalendarsAsync(cancellationToken);
                }

                foreach (var calendar in outlookCalendars)
                {
                    // Check if this calendar should be included
                    if (request.CalendarId != null && calendar.Id != request.CalendarId)
                    {
                        continue;
                    }

                    if (request.VisibleCalendarIds != null && !request.VisibleCalendarIds.Contains(calendar.Id))
                    {
                        continue;
                    }

                    // Extract the actual Outlook calendar ID from our ID format
                    var outlookCalendarId = calendar.Id.Replace("outlook_", "");

                    var outlookEvents = await outlookProvider.GetEventsAsync(
                        outlookCalendarId,
                        request.StartTime,
                        request.EndTime,
                        cancellationToken);

                    // Update CalendarId to use our internal ID format
                    foreach (var evt in outlookEvents)
                    {
                        evt.CalendarId = calendar.Id;
                    }

                    allEvents.AddRange(outlookEvents);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail if Outlook fetch fails
                //Log.Warning(ex, "Failed to fetch Outlook events, showing local events only");
            }
        }

        return allEvents;
    }

    /// <summary>
    /// Syncs Outlook calendars to the local store when connected but calendars don't exist yet.
    /// </summary>
    private async Task<List<Calendar>> SyncOutlookCalendarsAsync(CancellationToken cancellationToken)
    {
        var syncedCalendars = new List<Calendar>();

        try
        {
            var outlookCalendarInfos = await outlookProvider.GetCalendarsAsync(cancellationToken);

            foreach (var outlookCal in outlookCalendarInfos)
            {
                var calendar = new Calendar
                {
                    Id = $"outlook_{outlookCal.Id}",
                    Name = $"{outlookCal.Name} (Outlook)",
                    Color = outlookCal.Color,
                    Type = CalendarType.Microsoft,
                    EnableBlocking = true,
                    ReceiveBlocks = false
                };

                await calendarStore.AddAsync(calendar, cancellationToken);
                syncedCalendars.Add(calendar);
                // Log.Information("Auto-synced Outlook calendar: {CalendarName}", calendar.Name);
            }
        }
        catch (Exception ex)
        {
            // Log.Warning(ex, "Failed to auto-sync Outlook calendars");
        }

        return syncedCalendars;
    }
}
