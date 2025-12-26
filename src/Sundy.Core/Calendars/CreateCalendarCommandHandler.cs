using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class CreateCalendarCommandHandler(ICalendarStore store) : IRequestHandler<CreateCalendarCommand>
{

    public async ValueTask<Unit> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        await store.AddAsync(request.Calendar, cancellationToken);
        return Unit.Value;
    }
}
