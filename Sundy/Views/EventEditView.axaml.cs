using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.VisualTree;
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

            // Subscribe to new ViewModel
            viewModel.CalendarSelected += OnCalendarSelected;
            viewModel.OnSchedulerOpenRequested = OnSchedulerOpenRequested;
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

    private async Task OnSchedulerOpenRequested(EventEditViewModel viewModel)
    {
        // Check if we're running in a browser or desktop environment
        var topLevel = TopLevel.GetTopLevel(this);

        if (OperatingSystem.IsBrowser() || topLevel is not Window)
        {
            // Browser mode: Show scheduler in an overlay within the current view
            await ShowSchedulerOverlay(viewModel);
        }
        else
        {
            // Desktop mode: Show scheduler in a separate window
            await ShowSchedulerWindow(viewModel, topLevel as Window);
        }
    }

    private async Task ShowSchedulerWindow(EventEditViewModel viewModel, Window? owner)
    {
        var schedulerWindow = new SchedulerWindow(viewModel.Scheduler);

        // Handle confirm
        void OnConfirmed(object? s, EventArgs args)
        {
            schedulerWindow.Close();
        }

        viewModel.Scheduler.Confirmed += OnConfirmed;

        // Handle cancel
        viewModel.Scheduler.Cancelled += (_, _) =>
        {
            schedulerWindow.Close();
        };

        // Show dialog
        if (owner != null)
        {
            await schedulerWindow.ShowDialog(owner);
        }

        // Cleanup
        viewModel.Scheduler.Confirmed -= OnConfirmed;
    }

    private async Task ShowSchedulerOverlay(EventEditViewModel viewModel)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("ShowSchedulerOverlay: Method called");
#endif

        // Find the root grid in the parent MainView to add our overlay
        var topLevel = TopLevel.GetTopLevel(this);

#if DEBUG
        System.Diagnostics.Debug.WriteLine($"ShowSchedulerOverlay: TopLevel type = {topLevel?.GetType().Name ?? "null"}");
#endif

        // Navigate to find the MainView's root Grid
        Grid? rootGrid = null;

        if (topLevel?.Content is MainView mainView)
        {
            // Browser mode: Get the root Grid via Content property
            rootGrid = mainView.Content as Grid;
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"ShowSchedulerOverlay: Browser mode - rootGrid = {(rootGrid != null ? "found" : "null")}");
#endif
        }
        else if (topLevel is Window window && window.Content is MainView windowMainView)
        {
            // Desktop mode: Window -> MainView -> Grid
            rootGrid = windowMainView.Content as Grid;
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"ShowSchedulerOverlay: Desktop mode - rootGrid = {(rootGrid != null ? "found" : "null")}");
#endif
        }

        if (rootGrid == null)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("ShowSchedulerOverlay: Failed to find root Grid, returning");
#endif
            return;
        }

        // Create the overlay
        var overlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
            IsVisible = true,
            ZIndex = 1000,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

#if DEBUG
        System.Diagnostics.Debug.WriteLine("ShowSchedulerOverlay: Overlay Border created with Stretch alignment");
#endif

        // Create the scheduler control
        var schedulerControl = new SchedulerControl(viewModel.Scheduler)
        {
            Width = 500,
            Height = 700,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        overlay.Child = schedulerControl;

        // Add the overlay to the root grid
        rootGrid.Children.Add(overlay);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("ShowSchedulerOverlay: Overlay added to root Grid successfully");
#endif

        // Task completion source to wait for close
        var tcs = new TaskCompletionSource<bool>();

        // Handle confirm
        void OnConfirmed(object? s, EventArgs args)
        {
            // Apply the scheduler selection back to the event
            viewModel.ApplySchedulerSelection();
            rootGrid.Children.Remove(overlay);
            tcs.TrySetResult(true);
        }

        // Handle cancel
        void OnCancelled(object? s, EventArgs args)
        {
            rootGrid.Children.Remove(overlay);
            tcs.TrySetResult(false);
        }

        viewModel.Scheduler.Confirmed += OnConfirmed;
        viewModel.Scheduler.Cancelled += OnCancelled;

        // Handle clicking outside the dialog
        overlay.PointerPressed += (_, args) =>
        {
            // Only close if clicking the overlay itself, not the scheduler control
            if (ReferenceEquals(args.Source, overlay))
            {
                rootGrid.Children.Remove(overlay);
                tcs.TrySetResult(false);
            }
        };

        await tcs.Task;

        // Cleanup
        viewModel.Scheduler.Confirmed -= OnConfirmed;
        viewModel.Scheduler.Cancelled -= OnCancelled;
    }
}
