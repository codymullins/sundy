using Mediator;

namespace Sundy.Core.Queries;

public record GetEventByIdQuery(string EventId) : IRequest<CalendarEvent?>;
