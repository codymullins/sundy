using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class DeleteCalendarCommandHandler(IEventRepository repository) : IRequestHandler<DeleteCalendarCommand>
{

    public async ValueTask<Unit> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteCalendarAsync(request.CalendarId, cancellationToken);
        return Unit.Value;
    }
}
