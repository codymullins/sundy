using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class CalendarView : UserControl
{
    private DispatcherTimer? _timeUpdateTimer;

    public CalendarView()
    {
        InitializeComponent();

        LayoutUpdated += OnLayoutUpdated;
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateEventPositions();
        StartTimeUpdateTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopTimeUpdateTimer();
    }

    private void StartTimeUpdateTimer()
    {
        _timeUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timeUpdateTimer.Tick += OnTimeUpdateTick;
        _timeUpdateTimer.Start();

        // Update immediately
        if (DataContext is CalendarViewModel vm)
        {
            vm.UpdateCurrentTime();
        }
    }

    private void StopTimeUpdateTimer()
    {
        if (_timeUpdateTimer != null)
        {
            _timeUpdateTimer.Stop();
            _timeUpdateTimer.Tick -= OnTimeUpdateTick;
            _timeUpdateTimer = null;
        }
    }

    private void OnTimeUpdateTick(object? sender, EventArgs e)
    {
        if (DataContext is CalendarViewModel vm)
        {
            vm.UpdateCurrentTime();
        }
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CalendarViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(CalendarViewModel.IsWeekView) or nameof(CalendarViewModel.IsDayView) or nameof(CalendarViewModel.VisibleEvents))
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(UpdateEventPositions, Avalonia.Threading.DispatcherPriority.Render);
                }
            };
        }
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        UpdateEventPositions();
    }

    private void UpdateEventPositions()
    {
        if (DataContext is not CalendarViewModel vm) return;
        
        if (vm.IsWeekView)
        {
            var itemsControl = this.FindControl<ItemsControl>("WeekEventsItemsControl");
            if (itemsControl != null)
            {
                UpdateWeekEventPositions(itemsControl, vm);
            }
        }
        
        if (vm.IsDayView)
        {
            var itemsControl = this.FindControl<ItemsControl>("DayEventsItemsControl");
            if (itemsControl != null)
            {
                UpdateDayEventPositions(itemsControl, vm);
            }
        }
    }

    private void UpdateWeekEventPositions(ItemsControl itemsControl, CalendarViewModel vm)
    {
        var presenter = itemsControl.FindDescendantOfType<ItemsPresenter>();
        var panel = presenter?.Panel;
        
        if (panel == null) return;
        
        var canvasWidth = panel.Bounds.Width;
        if (canvasWidth <= 0) return;

        var dayWidth = canvasWidth / 7.0;
        
        foreach (var eventVm in vm.VisibleEvents)
        {
            var left = (eventVm.DayIndex * dayWidth) + 4;
            var width = Math.Max(20, dayWidth - 8);
            eventVm.UpdateCanvasPosition(left, width);
        }
    }

    private void UpdateDayEventPositions(ItemsControl itemsControl, CalendarViewModel vm)
    {
        var presenter = itemsControl.FindDescendantOfType<ItemsPresenter>();
        var panel = presenter?.Panel;
        
        if (panel == null) return;
        
        var canvasWidth = panel.Bounds.Width;
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
