using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class UpdateCalendarCommandHandler(
    ICalendarStore store,
    OperationRecorder operationRecorder) : IRequestHandler<UpdateCalendarCommand>
{
    public async ValueTask<Unit> Handle(UpdateCalendarCommand request, CancellationToken cancellationToken)
    {
        await store.UpdateCalendarAsync(request.Calendar, cancellationToken);

        // Record for sync
        await operationRecorder.RecordUpdateAsync(
            EntityType.Calendar,
            request.Calendar.Id,
            request.Calendar,
            cancellationToken);

        return Unit.Value;
    }
}
