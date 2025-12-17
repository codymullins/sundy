using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Test.TestHelpers;

namespace Sundy.Test.Core;

public class SQLiteEventStoreTests
{
    [Theory, Auto]
    public async Task GetEventsInRangeAsync_ReturnsEventsWithinRange(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event fully within range
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 10, 0, 0),
            new DateTime(2024, 1, 10, 11, 0, 0),
            "Event Within Range");
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal(evt.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_ExcludesEventsOutsideRange(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event before range
        var eventBefore = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 7, 0, 0),
            new DateTime(2024, 1, 10, 8, 0, 0));
        await SQLiteEventStore.CreateEventAsync(eventBefore);

        // Event after range
        var eventAfter = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 18, 0, 0),
            new DateTime(2024, 1, 10, 19, 0, 0));
        await SQLiteEventStore.CreateEventAsync(eventAfter);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_IncludesEventsOverlappingStartBoundary(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange - Tests line 11: e.StartTime < endTime && e.EndTime > startTime
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event starts before range, ends within range
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 8, 30, 0),
            new DateTime(2024, 1, 10, 10, 0, 0));
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert - Should be included because it overlaps the start
        Assert.Single(results);
        Assert.Equal(evt.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_IncludesEventsOverlappingEndBoundary(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange - Tests line 11: e.StartTime < endTime && e.EndTime > startTime
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event starts within range, ends after range
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 16, 30, 0),
            new DateTime(2024, 1, 10, 18, 0, 0));
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert - Should be included because it overlaps the end
        Assert.Single(results);
        Assert.Equal(evt.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_ExcludesEventsThatEndAtRangeStart(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange - Tests boundary condition: e.EndTime > startTime
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event ends exactly at range start
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 8, 0, 0),
            new DateTime(2024, 1, 10, 9, 0, 0)); // Ends at 9:00
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert - Should be excluded (EndTime = startTime is not > startTime)
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_ExcludesEventsThatStartAtRangeEnd(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange - Tests boundary condition: e.StartTime < endTime
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event starts exactly at range end
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 10, 17, 0, 0), // Starts at 17:00
            new DateTime(2024, 1, 10, 18, 0, 0));
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert - Should be excluded (StartTime = endTime is not < endTime)
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_HandlesMultiDayEventsSpanningEntireRange(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        // Event that spans entire day and beyond
        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 1, 9, 0, 0, 0),  // Day before
            new DateTime(2024, 1, 11, 23, 59, 0)); // Day after
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal(evt.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_FiltersByCalendarId(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        await mediator.Send(new CreateCalendarCommand(calendar1));
        await mediator.Send(new CreateCalendarCommand(calendar2));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        var event1 = TestDataBuilder.CreateTestEvent(
            calendar1.Id,
            rangeStart.AddHours(1),
            rangeStart.AddHours(2));
        var event2 = TestDataBuilder.CreateTestEvent(
            calendar2.Id,
            rangeStart.AddHours(1),
            rangeStart.AddHours(2));

        await SQLiteEventStore.CreateEventAsync(event1);
        await SQLiteEventStore.CreateEventAsync(event2);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar1.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal(event1.Id, results[0].Id);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_ReturnsAllCalendarsWhenCalendarIdIsNull(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        await mediator.Send(new CreateCalendarCommand(calendar1));
        await mediator.Send(new CreateCalendarCommand(calendar2));

        var rangeStart = new DateTime(2024, 1, 10, 9, 0, 0);
        var rangeEnd = new DateTime(2024, 1, 10, 17, 0, 0);

        var event1 = TestDataBuilder.CreateTestEvent(
            calendar1.Id,
            rangeStart.AddHours(1),
            rangeStart.AddHours(2));
        var event2 = TestDataBuilder.CreateTestEvent(
            calendar2.Id,
            rangeStart.AddHours(3),
            rangeStart.AddHours(4));

        await SQLiteEventStore.CreateEventAsync(event1);
        await SQLiteEventStore.CreateEventAsync(event2);

        // Act - null calendarId should return events from all calendars
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendarId: null);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Theory, Auto]
    public async Task CreateEventAsync_GeneratesIdWhenNull(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        evt.Id = null!; // Explicitly set to null

        // Act
        var created = await SQLiteEventStore.CreateEventAsync(evt);

        // Assert
        Assert.NotNull(created.Id);
        Assert.NotEmpty(created.Id);
    }

    [Theory, Auto]
    public async Task CreateEventAsync_PreservesIdWhenProvided(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var providedId = Guid.NewGuid().ToString();
        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        evt.Id = providedId;

        // Act
        var created = await SQLiteEventStore.CreateEventAsync(evt);

        // Assert
        Assert.Equal(providedId, created.Id);
    }

    [Theory, Auto]
    public async Task UpdateEventAsync_UpdatesExistingEvent(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id, title: "Original Title");
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        evt.Title = "Updated Title";
        await SQLiteEventStore.UpdateEventAsync(evt);

        var updated = await SQLiteEventStore.GetEventByIdAsync(evt.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
    }

    [Theory, Auto]
    public async Task DeleteEventAsync_RemovesEvent(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        await SQLiteEventStore.DeleteEventAsync(evt.Id);

        var deleted = await SQLiteEventStore.GetEventByIdAsync(evt.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Theory, Auto]
    public async Task DeleteEventAsync_DoesNothingWhenEventNotFound(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert - Should not throw
        await SQLiteEventStore.DeleteEventAsync(nonExistentId);
    }

    [Theory, Auto]
    public async Task GetEventByIdAsync_ReturnsEventWhenExists(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var evt = TestDataBuilder.CreateTestEvent(calendar.Id);
        await SQLiteEventStore.CreateEventAsync(evt);

        // Act
        var result = await SQLiteEventStore.GetEventByIdAsync(evt.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(evt.Id, result.Id);
        Assert.Equal(evt.Title, result.Title);
    }

    [Theory, Auto]
    public async Task GetEventsInRangeAsync_HandlesMidnightBoundaryEvents(IMediator mediator, SQLiteEventStore SQLiteEventStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await mediator.Send(new CreateCalendarCommand(calendar));

        var date = new DateTime(2024, 1, 10);
        var midnight = DateTimeTestHelpers.CreateMidnightDateTime(date);

        // Event at exactly midnight
        var evt = TestDataBuilder.CreateTestEventAtMidnight(calendar.Id, date);
        await SQLiteEventStore.CreateEventAsync(evt);

        var rangeStart = midnight;
        var rangeEnd = midnight.AddHours(12);

        // Act
        var results = await SQLiteEventStore.GetEventsInRangeAsync(
            new DateTimeOffset(rangeStart),
            new DateTimeOffset(rangeEnd),
            calendar.Id);

        // Assert
        Assert.Single(results);
    }
}
