using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class DeleteCalendarCommandHandler(ICalendarStore calendarStore, IEventStore eventStore) : IRequestHandler<DeleteCalendarCommand>
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
            }
        }

        // Then delete the calendar
        await calendarStore.DeleteCalendarAsync(request.CalendarId, cancellationToken);
        return Unit.Value;
    }
}
