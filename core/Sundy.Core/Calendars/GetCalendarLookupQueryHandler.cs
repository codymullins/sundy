using Mediator;
using Sundy.Core.Queries;

namespace Sundy.Core.Handlers;

public class GetCalendarLookupQueryHandler(ICalendarStore store)
    : IRequestHandler<GetCalendarLookupQuery, Dictionary<string, Calendar>>
{

    public async ValueTask<Dictionary<string, Calendar>> Handle(GetCalendarLookupQuery request, CancellationToken cancellationToken)
    {
        return await store.GetCalendarLookupAsync(cancellationToken);
    }
}
