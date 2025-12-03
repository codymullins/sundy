using Mediator;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetCalendarLookupQueryHandler(IEventRepository repository)
    : IRequestHandler<GetCalendarLookupQuery, Dictionary<string, Calendar>>
{

    public async ValueTask<Dictionary<string, Calendar>> Handle(GetCalendarLookupQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetCalendarLookupAsync(cancellationToken);
    }
}
