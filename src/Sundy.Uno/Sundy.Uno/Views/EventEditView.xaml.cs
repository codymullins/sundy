using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Sundy.Core;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Views;

/// <summary>
/// Event edit dialog - create and edit calendar events.
/// </summary>
public sealed partial class EventEditView : UserControl
{
    private ContentDialog? _schedulerDialog;

    public EventEditViewModel? ViewModel => DataContext as EventEditViewModel;

    public EventEditView()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (ViewModel != null)
        {
            ViewModel.SchedulerRequested -= OnSchedulerRequested;
            ViewModel.SchedulerRequested += OnSchedulerRequested;
        }
    }

    private void CalendarItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Calendar calendar && ViewModel != null)
        {
            ViewModel.SelectCalendarCommand.Execute(calendar);

            // Close the flyout
            if (CalendarSelectorButton.Flyout is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }

    private async void OnSchedulerRequested(object? sender, EventArgs e)
    {
        if (ViewModel == null || XamlRoot == null) return;

        _schedulerDialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };

        var schedulerVm = new DateTimeSchedulerViewModel(
            ViewModel.StartDate,
            ViewModel.StartTime,
            ViewModel.EndTime,
            onConfirm: (date, startTime, endTime) =>
            {
                ViewModel.ApplySchedulerSelection(date, startTime, endTime);
                _schedulerDialog?.Hide();
            },
            onCancel: () =>
            {
                _schedulerDialog?.Hide();
            });

        _schedulerDialog.Content = new DateTimeSchedulerView { DataContext = schedulerVm };

        await _schedulerDialog.ShowAsync();
    }
}
