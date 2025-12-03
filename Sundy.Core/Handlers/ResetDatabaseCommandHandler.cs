using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class ResetDatabaseCommandHandler(IEventRepository repository) : IRequestHandler<ResetDatabaseCommand>
{
    public async ValueTask<Unit> Handle(ResetDatabaseCommand request, CancellationToken cancellationToken)
    {
        await repository.ResetDatabaseAsync(cancellationToken);
        return Unit.Value;
    }
}
