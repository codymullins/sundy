using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class UpdateEventCommandHandler(
    IEventStore repository,
    OperationRecorder operationRecorder) : IRequestHandler<UpdateEventCommand, CalendarEvent>
{
    public async ValueTask<CalendarEvent> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        await repository.UpdateEventAsync(request.Event, cancellationToken);

        // Record for sync
        await operationRecorder.RecordUpdateAsync(
            EntityType.Event,
            request.Event.Id!,
            request.Event,
            cancellationToken);

        return request.Event;
    }
}
