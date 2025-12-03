using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class CreateCalendarCommandHandler(CalendarStore store) : IRequestHandler<CreateCalendarCommand>
{

    public async ValueTask<Unit> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        await store.CreateCalendarAsync(request.Calendar, cancellationToken);
        return Unit.Value;
    }
}
