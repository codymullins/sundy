using Mediator;

namespace Sundy.Core.Commands;

public record UpdateCalendarCommand(Calendar Calendar) : IRequest;
