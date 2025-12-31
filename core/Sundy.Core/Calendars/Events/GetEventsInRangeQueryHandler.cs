using Mediator;

using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

/// <summary>
/// Handler for getting events in a date range.
/// Supports multiple Microsoft accounts via IMicrosoftAccountManager.
/// </summary>
public class GetEventsInRangeQueryHandler(
    IEventStore repository,
    OutlookCalendarProvider outlookProvider,
    ICalendarStore calendarStore,
    IMicrosoftAccountManager accountManager)
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

        var allEvents = new List<CalendarEvent>(localEvents);

        // Get all Microsoft calendars we have registered
        var calendars = await calendarStore.GetAllAsync(cancellationToken);
        var outlookCalendars = calendars.Where(c => c.Type == CalendarType.Microsoft).ToList();

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

            try
            {
                // Extract the actual Outlook calendar ID from our ID format
                var graphCalendarId = calendar.Id.Replace("outlook_", "");

                var outlookEvents = await outlookProvider.GetEventsAsync(
                    calendar.ExternalAccountId,
                    graphCalendarId,
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
            catch (Exception)
            {
                // Log but don't fail if fetching events for one calendar fails
                // Continue with other calendars
            }
        }

        return allEvents;
    }
}
