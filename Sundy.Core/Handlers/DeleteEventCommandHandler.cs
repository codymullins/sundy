using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class DeleteEventCommandHandler(IEventRepository repository) : IRequestHandler<DeleteEventCommand>
{

    public async ValueTask<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteEventAsync(request.EventId, cancellationToken);
        return Unit.Value;
    }
}
