using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class DeleteCalendarCommandHandler(ICalendarStore store) : IRequestHandler<DeleteCalendarCommand>
{

    public async ValueTask<Unit> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        await store.DeleteCalendarAsync(request.CalendarId, cancellationToken);
        return Unit.Value;
    }
}
