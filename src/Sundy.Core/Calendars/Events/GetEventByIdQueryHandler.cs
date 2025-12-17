using Mediator;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetEventByIdQueryHandler(IEventStore repository) : IRequestHandler<GetEventByIdQuery, CalendarEvent?>
{
    public async ValueTask<CalendarEvent?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetEventByIdAsync(request.EventId, cancellationToken);
    }
}
