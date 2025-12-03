using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class CreateCalendarCommandHandler(IEventRepository repository) : IRequestHandler<CreateCalendarCommand>
{

    public async ValueTask<Unit> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        await repository.CreateCalendarAsync(request.Calendar, cancellationToken);
        return Unit.Value;
    }
}
