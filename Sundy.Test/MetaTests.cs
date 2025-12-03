using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;

namespace Sundy.Test;

public class MetaTests
{
    [Theory]
    [Auto]
    public async Task MetaStore_CanInitialize(IMediator mediator)
    {
        await mediator.Send(new InitializeDatabaseCommand());
        await mediator.Send(new CreateCalendarCommand(new Calendar
        {
            Id = "cal1", Name = "Test Calendar", Color = "#FF0000", Type = CalendarType.Local, EnableBlocking = true,
            ReceiveBlocks = true
        }));

        var calendars = await mediator.Send(new GetAllCalendarsQuery());
        Assert.Single(calendars);
    }

    [Theory]
    [Auto]
    public async Task MetaStore_CanReset(IMediator mediator)
    {
        await mediator.Send(new InitializeDatabaseCommand());
        await mediator.Send(new CreateCalendarCommand(new Calendar
        {
            Id = "cal1", Name = "Test Calendar", Color = "#FF0000", Type = CalendarType.Local, EnableBlocking = true,
            ReceiveBlocks = true
        }));

        var calendars = await mediator.Send(new GetAllCalendarsQuery());
        Assert.Single(calendars);
        
        // Now reset
        await mediator.Send(new ResetDatabaseCommand());
        calendars = await mediator.Send(new GetAllCalendarsQuery());
        Assert.Empty(calendars);
    }
}