using Mediator;
using Sundy.Core;
using Sundy.Test.TestHelpers;

namespace Sundy.Test.Core;

public class SQLiteCalendarStoreTests
{
    [Theory, Auto]
    public async Task CreateCalendarAsync_CreatesCalendar(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(name: "Test Calendar");

        // Act
        await SQLiteCalendarStore.CreateCalendarAsync(calendar);
        var retrieved = await SQLiteCalendarStore.GetAllAsync();

        // Assert
        Assert.Contains(retrieved, c => c.Id == calendar.Id);
    }

    [Theory, Auto]
    public async Task GetAllCalendarsAsync_ReturnsAllCalendars(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        var calendar3 = TestDataBuilder.CreateTestCalendar(name: "Calendar 3");

        await SQLiteCalendarStore.CreateCalendarAsync(calendar1);
        await SQLiteCalendarStore.CreateCalendarAsync(calendar2);
        await SQLiteCalendarStore.CreateCalendarAsync(calendar3);

        // Act
        var results = await SQLiteCalendarStore.GetAllAsync();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains(results, c => c.Id == calendar1.Id);
        Assert.Contains(results, c => c.Id == calendar2.Id);
        Assert.Contains(results, c => c.Id == calendar3.Id);
    }

    [Theory, Auto]
    public async Task GetAllCalendarsAsync_ReturnsEmptyWhenNoCalendars(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Act
        var results = await SQLiteCalendarStore.GetAllAsync();

        // Assert
        Assert.Empty(results);
    }

    [Theory, Auto]
    public async Task DeleteCalendarAsync_RemovesCalendar(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        await SQLiteCalendarStore.CreateCalendarAsync(calendar);

        // Act
        await SQLiteCalendarStore.DeleteCalendarAsync(calendar.Id);
        var remaining = await SQLiteCalendarStore.GetAllAsync();

        // Assert
        Assert.DoesNotContain(remaining, c => c.Id == calendar.Id);
    }

    [Theory, Auto]
    public async Task DeleteCalendarAsync_DoesNothingWhenCalendarNotFound(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert - Should not throw
        await SQLiteCalendarStore.DeleteCalendarAsync(nonExistentId);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupAsync_ReturnsDictionaryKeyedById(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");

        await SQLiteCalendarStore.CreateCalendarAsync(calendar1);
        await SQLiteCalendarStore.CreateCalendarAsync(calendar2);

        // Act
        var lookup = await SQLiteCalendarStore.GetCalendarLookupAsync();

        // Assert
        Assert.Equal(2, lookup.Count);
        Assert.True(lookup.ContainsKey(calendar1.Id));
        Assert.True(lookup.ContainsKey(calendar2.Id));
        Assert.Equal(calendar1.Name, lookup[calendar1.Id].Name);
        Assert.Equal(calendar2.Name, lookup[calendar2.Id].Name);
    }

    [Theory, Auto]
    public async Task GetCalendarLookupAsync_ReturnsEmptyDictionaryWhenNoCalendars(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Act
        var lookup = await SQLiteCalendarStore.GetCalendarLookupAsync();

        // Assert
        Assert.Empty(lookup);
    }

    [Theory, Auto]
    public async Task CreateCalendarAsync_WithAllProperties_PreservesData(IMediator mediator, SQLiteCalendarStore SQLiteCalendarStore)
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar(
            name: "My Work Calendar",
            color: "#FF5733",
            type: CalendarType.Local,
            enableBlocking: true,
            receiveBlocks: true);

        // Act
        await SQLiteCalendarStore.CreateCalendarAsync(calendar);
        var retrieved = (await SQLiteCalendarStore.GetAllAsync())
            .First(c => c.Id == calendar.Id);

        // Assert
        Assert.Equal("My Work Calendar", retrieved.Name);
        Assert.Equal("#FF5733", retrieved.Color);
        Assert.Equal(CalendarType.Local, retrieved.Type);
        Assert.True(retrieved.EnableBlocking);
        Assert.True(retrieved.ReceiveBlocks);
    }
}
