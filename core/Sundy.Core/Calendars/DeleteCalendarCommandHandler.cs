using Mediator;
using Sundy.Core.Commands;
using Sundy.Core.Sync;

namespace Sundy.Core.Handlers;

public class DeleteCalendarCommandHandler(
    ICalendarStore calendarStore,
    IEventStore eventStore,
    OperationRecorder operationRecorder) : IRequestHandler<DeleteCalendarCommand>
{
    public async ValueTask<Unit> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        // Delete all events associated with this calendar first
        var events = await eventStore.GetEventsInRangeAsync(
            DateTimeOffset.MinValue.AddYears(1), // SQLite doesn't handle MinValue well
            DateTimeOffset.MaxValue.AddYears(-1),
            request.CalendarId,
            ct: cancellationToken);

        foreach (var evt in events)
        {
            if (evt.Id != null)
            {
                await eventStore.DeleteEventAsync(evt.Id, cancellationToken);

                // Record event deletion for sync
                await operationRecorder.RecordDeleteAsync(
                    EntityType.Event,
                    evt.Id,
                    cancellationToken);
            }
        }

        // Then delete the calendar
        await calendarStore.DeleteCalendarAsync(request.CalendarId, cancellationToken);

        // Record calendar deletion for sync
        await operationRecorder.RecordDeleteAsync(
            EntityType.Calendar,
            request.CalendarId,
            cancellationToken);

        return Unit.Value;
    }
}
