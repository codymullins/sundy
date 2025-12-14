using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Mediator;
using Moq;
using Sundy.Avalonia.Test.TestHelpers;
using Sundy.Controls;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Core.System;
using Sundy.Test.TestHelpers;
using Sundy.ViewModels;
using Sundy.Views;

namespace Sundy.Avalonia.Test.Views;

public class EventEditViewTests : HeadlessViewTestBase
{
    private static EventTimeService CreateEventTimeService() => new(new SystemTimeZoneProvider());
    [AvaloniaFact]
    public async Task EventEditView_CreateMode_ShowsNewEventDialog()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();

        // Act
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Assert
        Assert.Equal("New Event", viewModel.DialogTitle);
        Assert.Equal("Create", viewModel.SaveButtonText);
        Assert.False(viewModel.IsEditMode);
    }

    [AvaloniaFact]
    public async Task EventEditView_EditMode_ShowsEditEventDialog()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var existingEvent = TestDataBuilder.CreateTestEvent(calendar.Id, title: "Existing Event");
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync(existingEvent);

        // Act
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Assert
        Assert.Equal("Edit Event", viewModel.DialogTitle);
        Assert.Equal("Save", viewModel.SaveButtonText);
        Assert.True(viewModel.IsEditMode);
        Assert.Equal("Existing Event", viewModel.Title);
    }

    [AvaloniaFact]
    public async Task EventEditView_TitleBinding_TwoWayBindingWorks()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find the title TextBox by watermark
        var titleTextBox = FindAllControlsByType<TextBox>(eventEditView)
            .FirstOrDefault(tb => tb.Watermark == "Event title");

        Assert.NotNull(titleTextBox);

        // Test ViewModel → UI
        viewModel.Title = "Team Meeting";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Team Meeting", titleTextBox.Text);

        // Test UI → ViewModel
        titleTextBox.Text = "Updated Meeting";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Updated Meeting", viewModel.Title);
    }

    [AvaloniaFact]
    public async Task EventEditView_CalendarSelectorButton_IsAccessible()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find the calendar selector button by name
        var calendarButton = FindControlByName<Button>(eventEditView, "CalendarSelectorButton");

        // Assert
        Assert.NotNull(calendarButton);
        Assert.NotNull(calendarButton.Flyout);
    }

    [AvaloniaFact]
    public async Task EventEditView_CalendarFlyout_CanBeOpened()
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Work");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Personal");
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar1, calendar2 });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find the calendar selector button
        var calendarButton = FindControlByName<Button>(eventEditView, "CalendarSelectorButton");
        Assert.NotNull(calendarButton);

        // Assert - Verify button has flyout with calendar items
        Assert.NotNull(calendarButton.Flyout);
        Assert.Equal(2, viewModel.AvailableCalendars.Count);
    }

    [AvaloniaFact]
    public async Task EventEditView_SelectCalendar_UpdatesViewModel()
    {
        // Arrange
        var calendar1 = TestDataBuilder.CreateTestCalendar(name: "Work");
        var calendar2 = TestDataBuilder.CreateTestCalendar(name: "Personal");
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar1, calendar2 });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Execute SelectCalendar command
        viewModel.SelectCalendarCommand.Execute(calendar2);
        Dispatcher.UIThread.RunJobs();

        // Assert
        Assert.Equal(calendar2, viewModel.SelectedCalendar);
        Assert.Equal("Personal", viewModel.SelectedCalendar?.Name);
    }

    [AvaloniaFact]
    public async Task EventEditView_AllDayCheckBox_BindsToViewModel()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find the All Day checkbox
        var allDayCheckBox = FindAllControlsByType<CheckBox>(eventEditView)
            .FirstOrDefault(cb => cb.Content?.ToString() == "All day event");

        Assert.NotNull(allDayCheckBox);

        // Set IsAllDay through ViewModel
        viewModel.IsAllDay = true;
        Dispatcher.UIThread.RunJobs();

        // Assert
        Assert.True(allDayCheckBox.IsChecked);
    }

    [AvaloniaFact]
    public async Task EventEditView_TimeSelectionPreview_DisplaysSchedulerData()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Set scheduler times
        viewModel.Scheduler.SelectedDate = new DateOnly(2024, 6, 15);
        viewModel.Scheduler.TimeBlock.StartTime = new TimeOnly(14, 30);
        viewModel.Scheduler.TimeBlock.EndTime = new TimeOnly(16, 0);
        Dispatcher.UIThread.RunJobs();

        // Assert - Verify TimeSelectionPreview exists
        var timePreview = FindControlByType<TimeSelectionPreview>(eventEditView);
        Assert.NotNull(timePreview);
    }

    [AvaloniaFact]
    public async Task EventEditView_BlockingEvent_DisablesControls()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var blockingEvent = TestDataBuilder.CreateTestEvent(calendar.Id, isBlockingEvent: true);
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync(blockingEvent);
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find controls that should be disabled
        var titleTextBox = FindAllControlsByType<TextBox>(eventEditView)
            .FirstOrDefault(tb => tb.Watermark == "Event title");
        var allDayCheckBox = FindAllControlsByType<CheckBox>(eventEditView)
            .FirstOrDefault(cb => cb.Content?.ToString() == "All day event");

        // Assert
        Assert.True(viewModel.IsBlockingEvent);
        Assert.NotNull(titleTextBox);
        Assert.NotNull(allDayCheckBox);
        Assert.False(titleTextBox.IsEnabled);
        Assert.False(allDayCheckBox.IsEnabled);
    }

    [AvaloniaFact]
    public async Task EventEditView_DeleteButton_VisibleInEditMode()
    {
        // Arrange
        var calendar = TestDataBuilder.CreateTestCalendar();
        var existingEvent = TestDataBuilder.CreateTestEvent(calendar.Id);
        var mediatorMock = CreateMediatorMock(new List<Core.Calendar> { calendar });
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync(existingEvent);
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find delete button by command
        var deleteButton = FindAllControlsByType<Button>(eventEditView)
            .FirstOrDefault(b => b.Command == viewModel.DeleteCommand);

        // Assert
        Assert.NotNull(deleteButton);
        Assert.True(deleteButton.IsVisible);
        Assert.Equal("Delete", deleteButton.Content);
    }

    [AvaloniaFact]
    public async Task EventEditView_DeleteButton_HiddenInCreateMode()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();
        var eventEditView = new EventEditView { DataContext = viewModel };
        var window = CreateTestWindow(eventEditView);

        // Act - Find delete button by command
        var deleteButton = FindAllControlsByType<Button>(eventEditView)
            .FirstOrDefault(b => b.Command == viewModel.DeleteCommand);

        // Assert
        Assert.NotNull(deleteButton);
        Assert.False(deleteButton.IsVisible);
    }

    [AvaloniaFact]
    public async Task EventEditView_SchedulerIntegration_ApplySelectionUpdatesEventTimes()
    {
        // Arrange
        var mediatorMock = CreateMediatorMock();
        var viewModel = new EventEditViewModel(mediatorMock.Object, CreateEventTimeService());
        await viewModel.InitializeAsync();

        // Act - Set up scheduler with specific date/time
        viewModel.Scheduler.SelectedDate = new DateOnly(2024, 6, 15);
        viewModel.Scheduler.TimeBlock.StartTime = new TimeOnly(14, 30);
        viewModel.Scheduler.TimeBlock.EndTime = new TimeOnly(16, 0);
        viewModel.ApplySchedulerSelection();

        // Assert - Verify event times updated from scheduler
        Assert.Equal(new DateTime(2024, 6, 15).Date, viewModel.StartDate.Date);
        Assert.Equal(new TimeSpan(14, 30, 0), viewModel.StartTime);
        Assert.Equal(new TimeSpan(16, 0, 0), viewModel.EndTime);
    }
}
