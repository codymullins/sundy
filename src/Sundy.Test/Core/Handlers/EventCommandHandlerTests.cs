using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Test.TestHelpers;

namespace Sundy.Test.Core.Handlers;

public class EventCommandHandlerTests
{
    [Theory, Auto]
    public async Task CreateEventCommandHandler_CreatesEvent(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id, title: "New Event");

        // Act
        await mediator.Send(new CreateEventCommand(evt));

        // Verify event was created
        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("New Event", retrieved.Title);
    }

    [Theory, Auto]
    public async Task CreateEventCommandHandler_GeneratesIdWhenNull(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        evt.Id = null!;

        // Act
        await mediator.Send(new CreateEventCommand(evt));

        // Event should have an ID now
        Assert.NotNull(evt.Id);
        Assert.NotEmpty(evt.Id);

        // Verify it was created
        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));
        Assert.NotNull(retrieved);
    }

    [Theory, Auto]
    public async Task CreateEventCommandHandler_PreservesProvidedId(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var providedId = Guid.NewGuid().ToString();
        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        evt.Id = providedId;

        // Act
        await mediator.Send(new CreateEventCommand(evt));

        // Verify ID was preserved
        var retrieved = await mediator.Send(new GetEventByIdQuery(providedId));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(providedId, retrieved.Id);
    }

    [Theory, Auto]
    public async Task UpdateEventCommandHandler_UpdatesEvent(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id, title: "Original");
        await mediator.Send(new CreateEventCommand(evt));

        // Act
        evt.Title = "Updated";
        await mediator.Send(new UpdateEventCommand(evt));

        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated", retrieved.Title);
    }

    [Theory, Auto]
    public async Task UpdateEventCommandHandler_UpdatesAllProperties(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            DateTime.Now,
            DateTime.Now.AddHours(1),
            title: "Original");
        await mediator.Send(new CreateEventCommand(evt));

        // Act - Update all properties
        var newStart = DateTime.Now.AddDays(1);
        var newEnd = newStart.AddHours(2);
        evt.Title = "Updated Title";
        evt.Location = "New Location";
        evt.Description = "New Description";
        evt.StartTime = new DateTimeOffset(newStart);
        evt.EndTime = new DateTimeOffset(newEnd);

        await mediator.Send(new UpdateEventCommand(evt));

        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Title", retrieved.Title);
        Assert.Equal("New Location", retrieved.Location);
        Assert.Equal("New Description", retrieved.Description);
        DateTimeTestHelpers.AssertDateTimeOffsetEqual(new DateTimeOffset(newStart), retrieved.StartTime);
        DateTimeTestHelpers.AssertDateTimeOffsetEqual(new DateTimeOffset(newEnd), retrieved.EndTime);
    }

    [Theory, Auto]
    public async Task DeleteEventCommandHandler_DeletesEvent(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        await mediator.Send(new CreateEventCommand(evt));

        // Act
        await mediator.Send(new DeleteEventCommand(evt.Id));

        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.Null(retrieved);
    }

    [Theory, Auto]
    public async Task DeleteEventCommandHandler_HandlesNonExistentEvent(IMediator mediator)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert - Should not throw
        await mediator.Send(new DeleteEventCommand(nonExistentId));
    }

    [Theory, Auto]
    public async Task GetEventByIdQueryHandler_ReturnsEvent(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id, title: "Find Me");
        await mediator.Send(new CreateEventCommand(evt));

        // Act
        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(evt.Id, retrieved.Id);
        Assert.Equal("Find Me", retrieved.Title);
    }

    [Theory, Auto]
    public async Task GetEventByIdQueryHandler_ReturnsNullWhenNotFound(IMediator mediator)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var retrieved = await mediator.Send(new GetEventByIdQuery(nonExistentId));

        // Assert
        Assert.Null(retrieved);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeQueryHandler_DelegatesToEventStore(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var start = new DateTime(2024, 1, 10, 9, 0, 0);
        var end = new DateTime(2024, 1, 10, 17, 0, 0);

        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            start.AddHours(1),
            start.AddHours(2));
        await mediator.Send(new CreateEventCommand(evt));

        // Act
        var results = await mediator.Send(new GetEventsInRangeQuery(
            new DateTimeOffset(start),
            new DateTimeOffset(end),
            calendar.Id));

        // Assert
        Assert.Single(results);
        Assert.Equal(evt.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeQueryHandler_FiltersCorrectly(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var start = new DateTime(2024, 1, 10, 9, 0, 0);
        var end = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event in range
        var inRangeEvent = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            start.AddHours(1),
            start.AddHours(2));

        // Event out of range
        var outOfRangeEvent = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            start.AddHours(-2),
            start.AddHours(-1));

        await mediator.Send(new CreateEventCommand(inRangeEvent));
        await mediator.Send(new CreateEventCommand(outOfRangeEvent));

        // Act
        var results = await mediator.Send(new GetEventsInRangeQuery(
            new DateTimeOffset(start),
            new DateTimeOffset(end),
            calendar.Id));

        // Assert
        Assert.Single(results);
        Assert.Equal(inRangeEvent.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task CreateEventCommandHandler_WithBlockingEvent_PreservesFlags(IMediator mediator)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var sourceEventId = Guid.NewGuid().ToString();
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            isBlockingEvent: true,
            sourceEventId: sourceEventId);

        // Act
        await mediator.Send(new CreateEventCommand(evt));

        var retrieved = await mediator.Send(new GetEventByIdQuery(evt.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.True(retrieved.IsBlockingEvent);
        Assert.Equal(sourceEventId, retrieved.SourceEventId);
    }
}
