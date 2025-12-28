using Mediator;

namespace Sundy.Core.Commands;

public record CreateEventCommand(CalendarEvent Event) : IRequest<CalendarEvent>;
