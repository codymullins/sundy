using Mediator;

namespace Sundy.Core.Meta;

public class ResetDatabaseCommandHandler(DapperDatabaseManager dbManager) : IRequestHandler<ResetDatabaseCommand>
{
    public async ValueTask<Unit> Handle(ResetDatabaseCommand request, CancellationToken cancellationToken)
    {
        await dbManager.DeleteDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await dbManager.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public class InitializeDatabaseCommandHandler(DapperDatabaseManager dbManager) : IRequestHandler<InitializeDatabaseCommand>
{
    public async ValueTask<Unit> Handle(InitializeDatabaseCommand request, CancellationToken cancellationToken)
    {
        await dbManager.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
