using Mediator;

namespace Sundy.Core.Queries;

public record GetEventsInRangeQuery(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? CalendarId = null,
    IReadOnlyList<string>? VisibleCalendarIds = null) : IRequest<List<CalendarEvent>>;
