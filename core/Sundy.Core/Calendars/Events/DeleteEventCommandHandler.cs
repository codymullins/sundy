using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class DeleteEventCommandHandler(
    IEventStore repository,
    OperationRecorder operationRecorder) : IRequestHandler<DeleteEventCommand>
{
    public async ValueTask<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteEventAsync(request.EventId, cancellationToken);

        // Record for sync
        await operationRecorder.RecordDeleteAsync(
            EntityType.Event,
            request.EventId,
            cancellationToken);

        return Unit.Value;
    }
}
