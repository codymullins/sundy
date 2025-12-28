using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Sundy.Avalonia.Test.TestHelpers;
using Sundy.Controls.Scheduler;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Avalonia.Test.Views;

public class SchedulerViewTests : HeadlessViewTestBase
{
    [AvaloniaFact]
    public void SchedulerView_Initializes_With_ViewModel_And_Displays_Week()
    {
        // Arrange & Act
        var viewModel = new SchedulerViewModel();
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Assert
        Assert.NotNull(schedulerView);
        Assert.Equal(7, viewModel.DayItems.Count);
        Assert.Contains(viewModel.DayItems, d => d.IsToday);
    }

    [AvaloniaFact]
    public void SchedulerView_NavigatePreviousWeek_UpdatesDayItems()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var initialDate = viewModel.SelectedDate;
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Execute the command directly
        viewModel.NavigatePreviousWeekCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        // Assert
        Assert.Equal(initialDate.AddDays(-7), viewModel.SelectedDate);
        Assert.Equal(7, viewModel.DayItems.Count);
    }

    [AvaloniaFact]
    public void SchedulerView_NavigateNextWeek_UpdatesDayItems()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var initialDate = viewModel.SelectedDate;
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Execute the command directly
        viewModel.NavigateNextWeekCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        // Assert
        Assert.Equal(initialDate.AddDays(7), viewModel.SelectedDate);
        Assert.Equal(7, viewModel.DayItems.Count);
    }

    [AvaloniaFact]
    public void SchedulerView_SelectDate_UpdatesSelectedDate()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);
        var targetDate = viewModel.DayItems[3].Date; // Select Wednesday

        // Act - Execute SelectDate command
        viewModel.SelectDateCommand.Execute(targetDate);
        Dispatcher.UIThread.RunJobs();

        // Assert
        Assert.Equal(targetDate, viewModel.SelectedDate);
        Assert.True(viewModel.DayItems[3].IsSelected);
        Assert.False(viewModel.DayItems[0].IsSelected);
    }

    [AvaloniaFact]
    public void SchedulerView_TimeBlock_PositionCalculatedCorrectly()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        viewModel.TimeBlock.StartTime = new TimeOnly(10, 30); // 10:30 AM
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Calculate expected Y position
        var expectedY = viewModel.TimeToY(viewModel.TimeBlock.StartTime);

        // Assert - Verify calculation is correct
        Assert.Equal(630.0, expectedY); // (10 * 60 + 30) * 1.0 = 630
    }

    [AvaloniaFact]
    public void SchedulerView_TimeBlock_DurationMatchesHeight()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        viewModel.TimeBlock.StartTime = new TimeOnly(14, 0);
        viewModel.TimeBlock.EndTime = new TimeOnly(16, 30); // 2.5 hour duration
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Calculate expected height
        var duration = viewModel.TimeBlock.Duration;
        var expectedHeight = duration.TotalMinutes * viewModel.PixelsPerMinute;

        // Assert
        Assert.Equal(TimeSpan.FromHours(2.5), duration);
        Assert.Equal(150.0, expectedHeight); // 150 minutes = 150 pixels at 1px/min
    }

    [AvaloniaFact]
    public void SchedulerView_CurrentTimeIndicator_PositionedCorrectly()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Set current time after view is initialized
        var testTime = new TimeOnly(9, 45);
        viewModel.CurrentTime = testTime;
        Dispatcher.UIThread.RunJobs();

        var expectedY = viewModel.TimeToY(testTime);

        // Assert
        Assert.Equal(585.0, expectedY); // (9 * 60 + 45) * 1.0 = 585
        Assert.Equal(testTime, viewModel.CurrentTime);
    }

    [AvaloniaFact]
    public void SchedulerView_ScrollViewer_IsAccessible()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);

        // Act - Find the named ScrollViewer
        var scrollViewer = FindControlByName<ScrollViewer>(schedulerView, "TimeGridScrollViewer");

        // Assert
        Assert.NotNull(scrollViewer);
    }

    [AvaloniaFact]
    public void SchedulerView_DataBinding_ViewModelPropertyChangesReflectInUI()
    {
        // Arrange
        var viewModel = new SchedulerViewModel();
        var schedulerView = new SchedulerView { DataContext = viewModel };
        var window = CreateTestWindow(schedulerView);
        var initialDate = viewModel.SelectedDate;

        // Act - Change ViewModel property
        viewModel.SelectedDate = initialDate.AddDays(7);
        Dispatcher.UIThread.RunJobs();

        // Assert - Verify DayItems collection updated
        Assert.Equal(7, viewModel.DayItems.Count);
        Assert.Equal(initialDate.AddDays(7), viewModel.DayItems[(int)initialDate.DayOfWeek].Date);
    }
}
