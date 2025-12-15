using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Query to get events from Outlook calendars for a date range.
/// </summary>
public record GetOutlookEventsQuery(
    DateTimeOffset Start,
    DateTimeOffset End,
    string? CalendarId = null // If null, gets events from all connected Outlook calendars
) : IQuery<List<CalendarEvent>>;

