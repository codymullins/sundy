using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class CreateEventCommandHandler(
    IEventStore repository,
    OperationRecorder operationRecorder) : IRequestHandler<CreateEventCommand, CalendarEvent>
{
    public async ValueTask<CalendarEvent> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await repository.CreateEventAsync(request.Event, cancellationToken);

        // Record for sync
        await operationRecorder.RecordInsertAsync(
            EntityType.Event,
            evt.Id!,
            evt,
            cancellationToken);

        return evt;
    }
}
