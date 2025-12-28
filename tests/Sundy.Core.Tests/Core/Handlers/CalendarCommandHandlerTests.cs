using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Test.TestHelpers;

namespace Sundy.Test.Core.Handlers;

public class CalendarCommandHandlerTests
{
    [Theory, Auto]
    public async Task CreateCalendarCommandHandler_CreatesCalendar(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(name: "Work Calendar");

        // Act
        await mediator.Send(new CreateCalendarCommand(calendar));

        var retrieved = await mediator.Send(new GetAllCalendarsQuery());

        // Assert
        Assert.Contains(retrieved, c => c.Id == calendar.Id && c.Name == "Work Calendar");
    }

    [Theory, Auto]
    public async Task CreateCalendarCommandHandler_WithAllProperties_PreservesData(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(
            name: "Personal Calendar",
            color: "#00FF00",
            type: CalendarType.Local,
            enableBlocking: true,
            receiveBlocks: true);

        // Act
        await mediator.Send(new CreateCalendarCommand(calendar));

        var retrieved = (await mediator.Send(new GetAllCalendarsQuery()))
            .First(c => c.Id == calendar.Id);

        // Assert
        Assert.Equal("Personal Calendar", retrieved.Name);
        Assert.Equal("#00FF00", retrieved.Color);
        Assert.Equal(CalendarType.Local, retrieved.Type);
        Assert.True(retrieved.EnableBlocking);
        Assert.True(retrieved.ReceiveBlocks);
    }

    [Theory, Auto]
    public async Task DeleteCalendarCommandHandler_DeletesCalendar(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        // Act
        await mediator.Send(new DeleteCalendarCommand(calendar.Id));

        var remaining = await mediator.Send(new GetAllCalendarsQuery());

        // Assert
        Assert.DoesNotContain(remaining, c => c.Id == calendar.Id);
    }

    [Theory, Auto]
    public async Task DeleteCalendarCommandHandler_HandlesNonExistentCalendar(IMediator mediator)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert - Should not throw
        await mediator.Send(new DeleteCalendarCommand(nonExistentId));
    }

    [Theory, Auto]
    public async Task GetAllCalendarsQueryHandler_ReturnsAllCalendars(IMediator mediator)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        var calendar3 = TestDataBuilder.CreateTestCalendar(name: "Calendar 3");

        await mediator.Send(new CreateCalendarCommand(calendar1));
        await mediator.Send(new CreateCalendarCommand(calendar2));
        await mediator.Send(new CreateCalendarCommand(calendar3));

        // Act
        var results = await mediator.Send(new GetAllCalendarsQuery());

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains(results, c => c.Name == "Calendar 1");
        Assert.Contains(results, c => c.Name == "Calendar 2");
        Assert.Contains(results, c => c.Name == "Calendar 3");
    }

    [Theory, Auto]
    public async Task GetAllCalendarsQueryHandler_ReturnsEmptyWhenNone(IMediator mediator)
    {
        // Act
        var results = await mediator.Send(new GetAllCalendarsQuery());

        // Assert
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupQueryHandler_ReturnsDictionary(IMediator mediator)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");

        await mediator.Send(new CreateCalendarCommand(calendar1));
        await mediator.Send(new CreateCalendarCommand(calendar2));

        // Act
        var lookup = await mediator.Send(new GetCalendarLookupQuery());

        // Assert
        Assert.Equal(2, lookup.Count);
        Assert.IsType<Dictionary<string, Calendar>>(lookup);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupQueryHandler_CorrectlyKeyedById(IMediator mediator)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");

        await mediator.Send(new CreateCalendarCommand(calendar1));
        await mediator.Send(new CreateCalendarCommand(calendar2));

        // Act
        var lookup = await mediator.Send(new GetCalendarLookupQuery());

        // Assert
        Assert.True(lookup.ContainsKey(calendar1.Id));
        Assert.True(lookup.ContainsKey(calendar2.Id));
        Assert.Equal("Calendar 1", lookup[calendar1.Id].Name);
        Assert.Equal("Calendar 2", lookup[calendar2.Id].Name);
    }

    [Theory, Auto]
    public async Task DeleteCalendarCommandHandler_WithEvents_DeletesCalendarAndEvents(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        // Create events for this calendar
        var event1 = TestDataBuilder.CreateTestEvent(calendar.Id);
        var event2 = TestDataBuilder.CreateTestEvent(calendar.Id);
        await mediator.Send(new CreateEventCommand(event1));
        await mediator.Send(new CreateEventCommand(event2));

        // Act - Delete the calendar
        await mediator.Send(new DeleteCalendarCommand(calendar.Id));

        // Assert - Calendar should be deleted
        var calendars = await mediator.Send(new GetAllCalendarsQuery());
        Assert.DoesNotContain(calendars, c => c.Id == calendar.Id);

        // Events should also be deleted (cascade delete)
        var evt1 = await mediator.Send(new GetEventByIdQuery(event1.Id));
        var evt2 = await mediator.Send(new GetEventByIdQuery(event2.Id));
        Assert.Null(evt1);
        Assert.Null(evt2);
    }

    [Theory, Auto]
    public async Task CreateCalendarCommandHandler_WithBlockingFlags_PreservesSettings(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(
            name: "Blocking Calendar",
            enableBlocking: true,
            receiveBlocks: false);

        // Act
        await mediator.Send(new CreateCalendarCommand(calendar));

        var retrieved = (await mediator.Send(new GetAllCalendarsQuery()))
            .First(c => c.Id == calendar.Id);

        // Assert
        Assert.True(retrieved.EnableBlocking);
        Assert.False(retrieved.ReceiveBlocks);
    }
}
