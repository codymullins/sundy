using Mediator;

namespace Sundy.Core.Queries;

public record GetAllCalendarsQuery : IRequest<List<Calendar>>;
