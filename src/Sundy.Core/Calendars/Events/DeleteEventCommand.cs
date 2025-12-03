using Mediator;

namespace Sundy.Core.Commands;

public record DeleteEventCommand(string EventId) : IRequest;
