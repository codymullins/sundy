using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class CalendarView : UserControl
{
    private Canvas? _weekEventsCanvas;
    private Canvas? _dayEventsCanvas;
    
    public CalendarView()
    {
        InitializeComponent();
        
        // Subscribe to layout updates
        LayoutUpdated += OnLayoutUpdated;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateEventPositions();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        UpdateEventPositions();
    }

    private void UpdateEventPositions()
    {
        // Find the WeekEventsCanvas if we're in week view
        _weekEventsCanvas ??= this.FindControl<Canvas>("WeekEventsCanvas");
        _dayEventsCanvas ??= this.FindControl<Canvas>("DayEventsCanvas");
        
        if (_weekEventsCanvas != null && DataContext is CalendarViewModel { IsWeekView: true } vm)
        {
            UpdateWeekEventPositions(_weekEventsCanvas, vm);
        }
        
        if (_dayEventsCanvas != null && DataContext is CalendarViewModel { IsDayView: true } dayVm)
        {
            UpdateDayEventPositions(_dayEventsCanvas, dayVm);
        }
    }

    private void UpdateWeekEventPositions(Canvas canvas, CalendarViewModel vm)
    {
        var canvasWidth = canvas.Bounds.Width;
        if (canvasWidth <= 0) return;

        var dayWidth = canvasWidth / 7.0;
        
        foreach (var eventVm in vm.VisibleEvents)
        {
            // Calculate position based on day index
            var left = (eventVm.DayIndex * dayWidth) + 4;
            var width = Math.Max(20, dayWidth - 8);
            
            // Update the event's properties for binding
            eventVm.UpdateCanvasPosition(left, width);
        }
    }

    private void UpdateDayEventPositions(Canvas canvas, CalendarViewModel vm)
    {
        var canvasWidth = canvas.Bounds.Width;
        if (canvasWidth <= 0) return;

        var width = Math.Max(20, canvasWidth - 12);
        
        foreach (var eventVm in vm.VisibleEvents)
        {
            eventVm.UpdateCanvasPosition(4, width);
        }
    }

    private void OnEventTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: EventViewModel eventVm })
        {
            eventVm.SelectCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnEventDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: EventViewModel eventVm })
        {
            eventVm.RequestEditCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnDayDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: MonthDayViewModel dayVm } && DataContext is CalendarViewModel calendarVm)
        {
            calendarVm.RequestNewEventForDateCommand.Execute(dayVm.Date);
            e.Handled = true;
        }
    }
}
