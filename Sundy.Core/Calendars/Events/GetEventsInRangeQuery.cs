using Mediator;

namespace Sundy.Core.Queries;

public record GetEventsInRangeQuery(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? CalendarId = null) : IRequest<List<CalendarEvent>>;
