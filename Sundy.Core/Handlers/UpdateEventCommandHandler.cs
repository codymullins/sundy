using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class UpdateEventCommandHandler(IEventRepository repository) : IRequestHandler<UpdateEventCommand, CalendarEvent>
{
    public async ValueTask<CalendarEvent> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        await repository.UpdateEventAsync(request.Event, cancellationToken);
        return request.Event;
    }
}
