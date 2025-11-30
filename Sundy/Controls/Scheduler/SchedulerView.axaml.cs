using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Controls.Scheduler;

public partial class SchedulerView : UserControl
{
    private readonly DispatcherTimer _timer;
    
    public SchedulerView()
    {
        InitializeComponent();
        
        DataContextChanged += (s, e) =>
        {
            Console.WriteLine("DataContext changed!");
            if (DataContext is SchedulerViewModel vm)
            {
                Console.WriteLine($"HourLines count: {vm.HourLines.Count}");
                Console.WriteLine($"PixelsPerHour: {vm.PixelsPerHour}");
                Console.WriteLine($"TotalHeight: {vm.TotalHeight}");
                foreach (var h in vm.HourLines.Take(5))
                {
                    Console.WriteLine($"  Hour {h.Hour}: Y={h.YPosition}, Label={h.Label}");
                }
            }
            else
            {
                Console.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
            }
        };
        
        Console.WriteLine("SchedulerView constructor - DataContext is:");
        Console.WriteLine(DataContext?.GetType().Name ?? "null");
        
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
}
