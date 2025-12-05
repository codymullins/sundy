using Mediator;
using Sundy.Core;
using Sundy.Test.TestHelpers;

namespace Sundy.Test.Core;

public class CalendarStoreTests
{
    [Theory, Auto]
    public async Task CreateCalendarAsync_CreatesCalendar(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(name: "Test Calendar");

        // Act
        await calendarStore.CreateCalendarAsync(calendar);
        var retrieved = await calendarStore.GetAllCalendarsAsync();

        // Assert
        Assert.Contains(retrieved, c => c.Id == calendar.Id);
    }

    [Theory, Auto]
    public async Task GetAllCalendarsAsync_ReturnsAllCalendars(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        var calendar3 = TestDataBuilder.CreateTestCalendar(name: "Calendar 3");

        await calendarStore.CreateCalendarAsync(calendar1);
        await calendarStore.CreateCalendarAsync(calendar2);
        await calendarStore.CreateCalendarAsync(calendar3);

        // Act
        var results = await calendarStore.GetAllCalendarsAsync();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains(results, c => c.Id == calendar1.Id);
        Assert.Contains(results, c => c.Id == calendar2.Id);
        Assert.Contains(results, c => c.Id == calendar3.Id);
    }

    [Theory, Auto]
    public async Task GetAllCalendarsAsync_ReturnsEmptyWhenNoCalendars(IMediator mediator, CalendarStore calendarStore)
    {
        // Act
        var results = await calendarStore.GetAllCalendarsAsync();

        // Assert
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task DeleteCalendarAsync_RemovesCalendar(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await calendarStore.CreateCalendarAsync(calendar);

        // Act
        await calendarStore.DeleteCalendarAsync(calendar.Id);
        var remaining = await calendarStore.GetAllCalendarsAsync();

        // Assert
        Assert.DoesNotContain(remaining, c => c.Id == calendar.Id);
    }

    [Theory, Auto]
    public async Task DeleteCalendarAsync_DoesNothingWhenCalendarNotFound(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert - Should not throw
        await calendarStore.DeleteCalendarAsync(nonExistentId);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupAsync_ReturnsDictionaryKeyedById(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");

        await calendarStore.CreateCalendarAsync(calendar1);
        await calendarStore.CreateCalendarAsync(calendar2);

        // Act
        var lookup = await calendarStore.GetCalendarLookupAsync();

        // Assert
        Assert.Equal(2, lookup.Count);
        Assert.True(lookup.ContainsKey(calendar1.Id));
        Assert.True(lookup.ContainsKey(calendar2.Id));
        Assert.Equal(calendar1.Name, lookup[calendar1.Id].Name);
        Assert.Equal(calendar2.Name, lookup[calendar2.Id].Name);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupAsync_ReturnsEmptyDictionaryWhenNoCalendars(IMediator mediator, CalendarStore calendarStore)
    {
        // Act
        var lookup = await calendarStore.GetCalendarLookupAsync();

        // Assert
        Assert.Empty(lookup);
    }

    [Theory, Auto]
    public async Task CreateCalendarAsync_WithAllProperties_PreservesData(IMediator mediator, CalendarStore calendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(
            name: "My Work Calendar",
            color: "#FF5733",
            type: CalendarType.Local,
            enableBlocking: true,
            receiveBlocks: true);

        // Act
        await calendarStore.CreateCalendarAsync(calendar);
        var retrieved = (await calendarStore.GetAllCalendarsAsync())
            .First(c => c.Id == calendar.Id);

        // Assert
        Assert.Equal("My Work Calendar", retrieved.Name);
        Assert.Equal("#FF5733", retrieved.Color);
        Assert.Equal(CalendarType.Local, retrieved.Type);
        Assert.True(retrieved.EnableBlocking);
        Assert.True(retrieved.ReceiveBlocks);
    }
}
