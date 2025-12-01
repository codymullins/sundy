using System;
using Avalonia.Controls;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class EventEditView : UserControl
{
    public EventEditView()
    {
        InitializeComponent();
        
        // Subscribe to DataContext changes to wire up the close flyout logic
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is EventEditViewModel viewModel)
        {
            // Unsubscribe from previous if needed
            viewModel.CalendarSelected -= OnCalendarSelected;
            viewModel.SchedulerOpenRequested -= OnSchedulerOpenRequested;
            
            // Subscribe to new ViewModel
            viewModel.CalendarSelected += OnCalendarSelected;
            viewModel.SchedulerOpenRequested += OnSchedulerOpenRequested;
        }
    }

    private void OnCalendarSelected(object? sender, EventArgs e)
    {
        // Close the flyout when a calendar is selected
        var button = this.FindControl<Button>("CalendarSelectorButton");
        if (button?.Flyout is Flyout flyout)
        {
            flyout.Hide();
        }
    }
    
    private async void OnSchedulerOpenRequested(object? sender, EventArgs e)
    {
        if (DataContext is not EventEditViewModel viewModel)
        {
            return;
        }
    
        if (viewModel.Scheduler.HourLines.Count > 0)
        {
            var first = viewModel.Scheduler.HourLines[0];
            var last = viewModel.Scheduler.HourLines[^1];
        }

        var schedulerWindow = new SchedulerWindow(viewModel.Scheduler);
        
        // Handle confirm
        void OnConfirmed(object? s, EventArgs args)
        {
            // Apply the scheduler selection back to the event
            viewModel.ApplySchedulerSelection();
            schedulerWindow.Close();
        }
        
        viewModel.Scheduler.Confirmed += OnConfirmed;
        
        // Handle cancel
        viewModel.Scheduler.Cancelled += (_, _) =>
        {
            schedulerWindow.Close();
        };
        
        // Show dialog
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window owner)
        {
            await schedulerWindow.ShowDialog(owner);
        }
        
        // Cleanup
        viewModel.Scheduler.Confirmed -= OnConfirmed;
    }
}
