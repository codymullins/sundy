using Mediator;

namespace Sundy.Core.Commands;

public record DeleteCalendarCommand(string CalendarId) : IRequest;
