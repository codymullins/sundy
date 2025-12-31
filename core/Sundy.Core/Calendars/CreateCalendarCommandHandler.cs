using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class CreateCalendarCommandHandler(
    ICalendarStore store,
    OperationRecorder operationRecorder) : IRequestHandler<CreateCalendarCommand>
{
    public async ValueTask<Unit> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        await store.AddAsync(request.Calendar, cancellationToken);

        // Record for sync
        await operationRecorder.RecordInsertAsync(
            EntityType.Calendar,
            request.Calendar.Id,
            request.Calendar,
            cancellationToken);

        return Unit.Value;
    }
}
