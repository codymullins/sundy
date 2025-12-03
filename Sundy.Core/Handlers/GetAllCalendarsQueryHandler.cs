using Mediator;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetAllCalendarsQueryHandler(IEventRepository repository)
    : IRequestHandler<GetAllCalendarsQuery, List<Calendar>>
{

    public async ValueTask<List<Calendar>> Handle(GetAllCalendarsQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAllCalendarsAsync(cancellationToken);
    }
}
