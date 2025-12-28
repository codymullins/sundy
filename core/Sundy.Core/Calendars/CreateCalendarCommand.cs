using Mediator;

namespace Sundy.Core.Commands;

public record CreateCalendarCommand(Calendar Calendar) : IRequest;
