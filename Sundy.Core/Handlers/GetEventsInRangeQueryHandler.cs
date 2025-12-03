using Mediator;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetEventsInRangeQueryHandler(IEventRepository repository)
    : IRequestHandler<GetEventsInRangeQuery, List<CalendarEvent>>
{
    public async ValueTask<List<CalendarEvent>> Handle(GetEventsInRangeQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetEventsInRangeAsync(
            request.StartTime,
            request.EndTime,
            request.CalendarId,
            cancellationToken);
    }
}
