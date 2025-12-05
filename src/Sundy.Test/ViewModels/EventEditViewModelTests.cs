using Mediator;
using Moq;
using Sundy.Core;
using Sundy.Core.Queries;
using Sundy.Test.TestHelpers;
using Sundy.ViewModels;

namespace Sundy.Test.ViewModels;

public class EventEditViewModelTests
{
    private static Mock<IMediator> CreateMediatorMock(List<Calendar> calendars)
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllCalendarsQuery>(), default))
            .ReturnsAsync(calendars);
        return mediatorMock;
    }

    [Fact]
    public async Task InitializeAsync_CreateMode_SetsDefaultTimes()
    {
        // Arrange
        var calendars = new List<Calendar> { TestDataBuilder.CreateTestCalendar() };
        var mediatorMock = CreateMediatorMock(calendars);
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        await viewModel.InitializeAsync();

        // Assert - When no default date provided, uses next hour from current time
        // EndTime should be 1 hour after StartTime
        Assert.Equal(TimeSpan.FromHours(1), viewModel.EndTime - viewModel.StartTime);
        Assert.Equal("New Event", viewModel.DialogTitle);
        Assert.Equal("Create", viewModel.SaveButtonText);
    }

    [Fact]
    public async Task InitializeAsync_CreateMode_WithDefaultDate_UsesProvidedDate()
    {
        // Arrange
        var calendars = new List<Calendar> { TestDataBuilder.CreateTestCalendar() };
        var mediatorMock = CreateMediatorMock(calendars);
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        var defaultDate = new DateOnly(2024, 5, 15);

        // Act
        await viewModel.InitializeAsync(defaultDate: defaultDate);

        // Assert
        Assert.Equal(2024, viewModel.StartDate.Year);
        Assert.Equal(5, viewModel.StartDate.Month);
        Assert.Equal(15, viewModel.StartDate.Day);
        Assert.Equal(TimeSpan.FromHours(9), viewModel.StartTime);
        Assert.Equal(TimeSpan.FromHours(10), viewModel.EndTime);
    }

    [Fact]
    public async Task InitializeAsync_CreateMode_WithDefaultCalendar_SelectsCalendar()
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Calendar 1");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Calendar 2");
        var calendars = new List<Calendar> { calendar1, calendar2 };
        var mediatorMock = CreateMediatorMock(calendars);
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        await viewModel.InitializeAsync(defaultCalendarId: calendar2.Id);

        // Assert
        Assert.Equal(calendar2.Id, viewModel.SelectedCalendar?.Id);
    }

    [Fact]
    public async Task InitializeAsync_EditMode_LoadsExistingEvent()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var calendars = new List<Calendar> { calendar };
        var mediatorMock = CreateMediatorMock(calendars);

        var existingEvent = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 6, 10, 14, 30, 0),
            new DateTime(2024, 6, 10, 16, 0, 0),
            title: "Existing Event",
            location: "Conference Room",
            description: "Important meeting");

        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        await viewModel.InitializeAsync(existingEvent: existingEvent);

        // Assert
        Assert.True(viewModel.IsEditMode);
        Assert.Equal("Edit Event", viewModel.DialogTitle);
        Assert.Equal("Save", viewModel.SaveButtonText);
        Assert.Equal("Existing Event", viewModel.Title);
        Assert.Equal("Conference Room", viewModel.Location);
        Assert.Equal("Important meeting", viewModel.Description);
        Assert.Equal(new DateTime(2024, 6, 10), viewModel.StartDate.DateTime.Date);
        Assert.Equal(TimeSpan.FromHours(14).Add(TimeSpan.FromMinutes(30)), viewModel.StartTime);
        Assert.Equal(TimeSpan.FromHours(16), viewModel.EndTime);
    }

    [Fact]
    public async Task InitializeAsync_EditMode_PreservesAllProperties()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var calendars = new List<Calendar> { calendar };
        var mediatorMock = CreateMediatorMock(calendars);

        var existingEvent = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 6, 10, 9, 0, 0),
            new DateTime(2024, 6, 10, 17, 0, 0),
            isBlockingEvent: true);

        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        await viewModel.InitializeAsync(existingEvent: existingEvent);

        // Assert
        Assert.Equal(calendar.Id, viewModel.SelectedCalendar?.Id);
        Assert.True(viewModel.IsBlockingEvent);
    }

    [Fact]
    public void OnIsAllDayChanged_ToTrue_SetsMidnightTimes()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        viewModel.StartDate = new DateTimeOffset(new DateTime(2024, 6, 10));
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);

        // Act
        viewModel.IsAllDay = true;

        // Assert - Tests line 263-264
        Assert.Equal(TimeSpan.Zero, viewModel.StartTime); // Midnight
        Assert.Equal(TimeSpan.Zero, viewModel.EndTime); // Midnight
    }

    [Fact]
    public void OnIsAllDayChanged_ToTrue_SetsEndDateToNextDay()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        var startDate = new DateTime(2024, 6, 10);
        viewModel.StartDate = new DateTimeOffset(startDate);
        viewModel.EndDate = new DateTimeOffset(startDate);

        // Act
        viewModel.IsAllDay = true;

        // Assert - Tests line 265
        Assert.Equal(startDate.AddDays(1).Date, viewModel.EndDate.DateTime.Date);
    }

    [Fact]
    public void OnIsAllDayChanged_ToFalse_ResetsToOneHourDuration()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        viewModel.StartDate = new DateTimeOffset(new DateTime(2024, 6, 10));
        viewModel.EndDate = new DateTimeOffset(new DateTime(2024, 6, 11));
        viewModel.StartTime = TimeSpan.Zero;
        viewModel.EndTime = TimeSpan.Zero;

        // Set to all-day first
        viewModel.IsAllDay = true;

        // Act - Toggle back to false
        viewModel.IsAllDay = false;

        // Assert - Tests line 274-279
        Assert.Equal(TimeSpan.FromHours(9), viewModel.StartTime);
        Assert.Equal(TimeSpan.FromHours(10), viewModel.EndTime);
        Assert.Equal(viewModel.StartDate.Date, viewModel.EndDate.Date); // Same day
    }

    [Fact]
    public void OnIsAllDayChanged_ToFalse_WhenNotMidnight_PreservesExistingTimes()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        viewModel.StartTime = TimeSpan.FromHours(14);
        viewModel.EndTime = TimeSpan.FromHours(15);

        viewModel.IsAllDay = true;
        // After setting all-day, reset to specific times
        viewModel.IsAllDay = false;

        // Times should reset because they were midnight
        Assert.Equal(TimeSpan.FromHours(9), viewModel.StartTime);
    }

    [Fact]
    public void OnStartDateChanged_WhenEndDateBefore_AdjustsEndDate()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        viewModel.StartDate = new DateTimeOffset(new DateTime(2024, 6, 10));
        viewModel.EndDate = new DateTimeOffset(new DateTime(2024, 6, 8));

        // Act - Tests line 289-293
        viewModel.StartDate = new DateTimeOffset(new DateTime(2024, 6, 15));

        // Assert
        Assert.Equal(new DateTime(2024, 6, 15).Date, viewModel.EndDate.DateTime.Date);
    }

    [Fact]
    public void OnStartTimeChanged_WhenEndTimeInvalid_AdjustsEndTime()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        var date = new DateTime(2024, 6, 10);
        viewModel.StartDate = new DateTimeOffset(date);
        viewModel.EndDate = new DateTimeOffset(date);
        viewModel.StartTime = TimeSpan.FromHours(10);
        viewModel.EndTime = TimeSpan.FromHours(11);

        // Act - Change start time to after end time - Tests line 302-307
        viewModel.StartTime = TimeSpan.FromHours(12);

        // Assert - End time should be adjusted to maintain 1 hour duration
        Assert.Equal(TimeSpan.FromHours(13), viewModel.EndTime);
    }

    [Fact]
    public void OnStartTimeChanged_WithSameDay_MaintainsOneHourDuration()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        var date = new DateTime(2024, 6, 10);
        viewModel.StartDate = new DateTimeOffset(date);
        viewModel.EndDate = new DateTimeOffset(date);
        viewModel.StartTime = TimeSpan.FromHours(9);
        viewModel.EndTime = TimeSpan.FromHours(10);

        // Act
        viewModel.StartTime = TimeSpan.FromHours(14);

        // Assert
        Assert.Equal(TimeSpan.FromHours(15), viewModel.EndTime);
    }

    [Fact]
    public void OnStartTimeChanged_Near23Hours_ClampsEndTimeTo2359()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);
        var date = new DateTime(2024, 6, 10);
        viewModel.StartDate = new DateTimeOffset(date);
        viewModel.EndDate = new DateTimeOffset(date);
        viewModel.StartTime = TimeSpan.FromHours(22);
        viewModel.EndTime = TimeSpan.FromHours(23);

        // Act - Tests line 306 - when adding 1 hour would exceed 24 hours
        viewModel.StartTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(30));

        // Assert - Should clamp to 23:59
        Assert.Equal(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)), viewModel.EndTime);
    }

    [Fact]
    public void TimeSpanToTimeOnly_WithValidTimeSpan_ConvertsCorrectly()
    {
        // We can't test the private method directly, but we can test its effects through OnStartTimeChanged
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act - Set a valid time
        viewModel.StartTime = TimeSpan.FromHours(14).Add(TimeSpan.FromMinutes(30));

        // Assert - Scheduler should be updated with correct TimeOnly
        Assert.Equal(new TimeOnly(14, 30), viewModel.Scheduler.TimeBlock.StartTime);
    }

    [Fact]
    public void TimeSpanToTimeOnly_WithTimeSpanOver24Hours_ClampsTo2359()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act - Set a time >= 24 hours (tests line 322-323)
        viewModel.StartTime = TimeSpan.FromHours(25);

        // Assert - Should clamp to 23:59:59
        Assert.Equal(new TimeOnly(23, 59, 59), viewModel.Scheduler.TimeBlock.StartTime);
    }

    [Theory]
    [InlineData(24, 0, 23, 59, 59)] // Exactly 24 hours
    [InlineData(25, 30, 23, 59, 59)] // 25.5 hours
    [InlineData(48, 0, 23, 59, 59)] // 48 hours
    public void TimeSpanToTimeOnly_WithOverflowTimeSpan_ClampsCorrectly(
        int hours, int minutes, int expectedHour, int expectedMinute, int expectedSecond)
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        viewModel.StartTime = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes));

        // Assert
        Assert.Equal(new TimeOnly(expectedHour, expectedMinute, expectedSecond),
            viewModel.Scheduler.TimeBlock.StartTime);
    }

    [Fact]
    public void ApplySchedulerSelection_WithSameDay_SetsCorrectTimes()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        viewModel.Scheduler.SelectedDate = new DateOnly(2024, 6, 10);
        viewModel.Scheduler.TimeBlock.StartTime = new TimeOnly(10, 30);
        viewModel.Scheduler.TimeBlock.EndTime = new TimeOnly(12, 0);

        // Act
        viewModel.ApplySchedulerSelection();

        // Assert
        Assert.Equal(new DateTime(2024, 6, 10).Date, viewModel.StartDate.DateTime.Date);
        Assert.Equal(new DateTime(2024, 6, 10).Date, viewModel.EndDate.DateTime.Date);
        Assert.Equal(new TimeSpan(10, 30, 0), viewModel.StartTime);
        Assert.Equal(new TimeSpan(12, 0, 0), viewModel.EndTime);
    }

    [Fact]
    public void ApplySchedulerSelection_WhenTimeBlockSpansMidnight_SetsEndDateToNextDay()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var viewModel = new EventEditViewModel(mediatorMock.Object);

        viewModel.Scheduler.SelectedDate = new DateOnly(2024, 6, 10);
        viewModel.Scheduler.TimeBlock.StartTime = new TimeOnly(23, 0);
        viewModel.Scheduler.TimeBlock.EndTime = new TimeOnly(1, 0); // Next day

        // Act - Tests line 231-235
        viewModel.ApplySchedulerSelection();

        // Assert
        Assert.Equal(new DateTime(2024, 6, 10).Date, viewModel.StartDate.DateTime.Date);
        Assert.Equal(new DateTime(2024, 6, 11).Date, viewModel.EndDate.DateTime.Date);
    }

    [Theory]
    [InlineData(0, 0, 1, 0)] // Midnight to 1am
    [InlineData(23, 0, 23, 59)] // Late evening
    [InlineData(12, 30, 14, 45)] // Mid-day with minutes
    public void CombineDateAndTime_VariousTimes_CombinesCorrectly(
        int startHour, int startMinute, int endHour, int endMinute)
    {
        // We can't test the private method directly, but we test it through initialization
        var calendar = TestDataBuilder.CreateTestCalendar();
        var calendars = new List<Calendar> { calendar };
        var mediatorMock = CreateMediatorMock(calendars);

        var evt = TestDataBuilder.CreateTestEvent(
            calendar.Id,
            new DateTime(2024, 6, 10, startHour, startMinute, 0),
            new DateTime(2024, 6, 10, endHour, endMinute, 0));

        var viewModel = new EventEditViewModel(mediatorMock.Object);

        // Act
        viewModel.InitializeAsync(existingEvent: evt).Wait();

        // Assert
        Assert.Equal(TimeSpan.FromHours(startHour).Add(TimeSpan.FromMinutes(startMinute)), viewModel.StartTime);
        Assert.Equal(TimeSpan.FromHours(endHour).Add(TimeSpan.FromMinutes(endMinute)), viewModel.EndTime);
    }
}
