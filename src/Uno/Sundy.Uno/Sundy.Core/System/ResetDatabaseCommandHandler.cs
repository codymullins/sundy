using Mediator;
using Sundy.Core.Commands;

namespace Sundy.Core.Handlers;

public class ResetDatabaseCommandHandler(DatabaseManager store) : IRequestHandler<ResetDatabaseCommand>
{
    public async ValueTask<Unit> Handle(ResetDatabaseCommand request, CancellationToken cancellationToken)
    {
        await store.DeleteDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await store.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public class InitializeDatabaseCommandHandler(DatabaseManager store) : IRequestHandler<InitializeDatabaseCommand>
{
    public async ValueTask<Unit> Handle(InitializeDatabaseCommand request, CancellationToken cancellationToken)
    {
        await store.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}