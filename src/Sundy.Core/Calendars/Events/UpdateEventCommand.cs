using Mediator;

namespace Sundy.Core.Commands;

public record UpdateEventCommand(CalendarEvent Event) : IRequest<CalendarEvent>;
