using Mediator;

namespace Sundy.Core.Queries;

public record GetCalendarLookupQuery : IRequest<Dictionary<string, Calendar>>;
