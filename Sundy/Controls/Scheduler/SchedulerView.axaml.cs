using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Controls.Scheduler;

public partial class SchedulerView : UserControl
{
    private readonly DispatcherTimer _timer;
    private bool _hasScrolledToTime = false;
    
    public SchedulerView()
    {
        InitializeComponent();
        
        DataContextChanged += (_, _) =>
        {
            // Reset scroll flag when DataContext changes
            _hasScrolledToTime = false;
        };
        
        // Set up timer to update current time indicator
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _timer.Tick += OnTimerTick;
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _timer.Start();
        UpdateCurrentTime();
        
        // Scroll to current time after a short delay to ensure layout is complete
        if (!_hasScrolledToTime)
        {
            Dispatcher.UIThread.Post(() => ScrollToCurrentTime(), DispatcherPriority.Loaded);
        }
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateCurrentTime();
    }

    private void UpdateCurrentTime()
    {
        if (DataContext is SchedulerViewModel viewModel)
        {
            viewModel.UpdateCurrentTime();
        }
    }
    
    private void ScrollToCurrentTime()
    {
        if (DataContext is not SchedulerViewModel viewModel)
            return;
            
        _hasScrolledToTime = true;
        
        // Get the Y position of the TimeBlock's start time
        var targetTime = viewModel.TimeBlock.StartTime;
        var targetY = viewModel.TimeToY(targetTime);
        
        // Center the time block in the viewport by offsetting by half the viewport height
        var scrollViewer = this.FindControl<ScrollViewer>("TimeGridScrollViewer");
        if (scrollViewer != null)
        {
            // Offset to center the time block, but ensure we show some context above
            var offset = Math.Max(0, targetY - 100);
            
            Console.WriteLine($"Scrolling to time {targetTime}, Y position: {targetY}, offset: {offset}");
            scrollViewer.Offset = new Avalonia.Vector(0, offset);
        }
    }
}
