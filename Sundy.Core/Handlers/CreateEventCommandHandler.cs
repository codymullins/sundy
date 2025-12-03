using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class CreateEventCommandHandler(IEventRepository repository) : IRequestHandler<CreateEventCommand, CalendarEvent>
{

    public async ValueTask<CalendarEvent> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        return await repository.CreateEventAsync(request.Event, cancellationToken);
    }
}
