using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Controls.Scheduler;

public class TimeBlockControl : TemplatedControl
{
    private bool _isDraggingBody;
    private bool _isDraggingTop;
    private bool _isDraggingBottom;
    private double _dragStartY;
    private TimeOnly _dragStartTime;
    private TimeOnly _dragEndTime;
    private Control? _topHandle;
    private Control? _bottomHandle;
    private Control? _bodyArea;

    public static readonly StyledProperty<TimeBlockViewModel?> TimeBlockProperty =
        AvaloniaProperty.Register<TimeBlockControl, TimeBlockViewModel?>(nameof(TimeBlock));

    public static readonly StyledProperty<double> PixelsPerMinuteProperty =
        AvaloniaProperty.Register<TimeBlockControl, double>(nameof(PixelsPerMinute), 1.0);

    public static readonly StyledProperty<IBrush?> BlockBrushProperty =
        AvaloniaProperty.Register<TimeBlockControl, IBrush?>(nameof(BlockBrush));

    public static readonly StyledProperty<IBrush?> HandleBrushProperty =
        AvaloniaProperty.Register<TimeBlockControl, IBrush?>(nameof(HandleBrush));

    public TimeBlockViewModel? TimeBlock
    {
        get => GetValue(TimeBlockProperty);
        set => SetValue(TimeBlockProperty, value);
    }

    public double PixelsPerMinute
    {
        get => GetValue(PixelsPerMinuteProperty);
        set => SetValue(PixelsPerMinuteProperty, value);
    }

    public IBrush? BlockBrush
    {
        get => GetValue(BlockBrushProperty);
        set => SetValue(BlockBrushProperty, value);
    }

    public IBrush? HandleBrush
    {
        get => GetValue(HandleBrushProperty);
        set => SetValue(HandleBrushProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Unsubscribe from old handles
        if (_topHandle != null)
        {
            _topHandle.PointerPressed -= OnTopHandlePressed;
            _topHandle.PointerMoved -= OnTopHandleMoved;
            _topHandle.PointerReleased -= OnHandleReleased;
        }

        if (_bottomHandle != null)
        {
            _bottomHandle.PointerPressed -= OnBottomHandlePressed;
            _bottomHandle.PointerMoved -= OnBottomHandleMoved;
            _bottomHandle.PointerReleased -= OnHandleReleased;
        }

        if (_bodyArea != null)
        {
            _bodyArea.PointerPressed -= OnBodyPressed;
            _bodyArea.PointerMoved -= OnBodyMoved;
            _bodyArea.PointerReleased -= OnBodyReleased;
        }

        // Get template parts
        _topHandle = e.NameScope.Find<Control>("PART_TopHandle");
        _bottomHandle = e.NameScope.Find<Control>("PART_BottomHandle");
        _bodyArea = e.NameScope.Find<Control>("PART_Body");

        // Subscribe to new handles
        if (_topHandle != null)
        {
            _topHandle.PointerPressed += OnTopHandlePressed;
            _topHandle.PointerMoved += OnTopHandleMoved;
            _topHandle.PointerReleased += OnHandleReleased;
        }

        if (_bottomHandle != null)
        {
            _bottomHandle.PointerPressed += OnBottomHandlePressed;
            _bottomHandle.PointerMoved += OnBottomHandleMoved;
            _bottomHandle.PointerReleased += OnHandleReleased;
        }

        if (_bodyArea != null)
        {
            _bodyArea.PointerPressed += OnBodyPressed;
            _bodyArea.PointerMoved += OnBodyMoved;
            _bodyArea.PointerReleased += OnBodyReleased;
        }
    }

    private void OnBodyPressed(object? sender, PointerPressedEventArgs e)
    {
        if (TimeBlock == null || sender is not Control control) return;
        
        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed) return;

        _isDraggingBody = true;
        _dragStartY = e.GetPosition(Parent as Visual).Y;
        _dragStartTime = TimeBlock.StartTime;
        _dragEndTime = TimeBlock.EndTime;
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void OnBodyMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingBody || TimeBlock == null || Parent is not Visual parent) return;

        var currentY = e.GetPosition(parent).Y;
        var deltaY = currentY - _dragStartY;
        var deltaMinutes = deltaY / PixelsPerMinute;
        
        var newStart = _dragStartTime.Add(TimeSpan.FromMinutes(deltaMinutes));
        TimeBlock.MoveToTime(newStart);
        e.Handled = true;
    }

    private void OnBodyReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingBody)
        {
            _isDraggingBody = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void OnTopHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (TimeBlock == null || sender is not Control control) return;
        
        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed) return;

        _isDraggingTop = true;
        _dragStartY = e.GetPosition(Parent as Visual).Y;
        _dragStartTime = TimeBlock.StartTime;
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void OnTopHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingTop || TimeBlock == null || Parent is not Visual parent) return;

        var currentY = e.GetPosition(parent).Y;
        var newStartTime = YToTime(currentY);
        TimeBlock.SetStartTime(newStartTime);
        e.Handled = true;
    }

    private void OnBottomHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (TimeBlock == null || sender is not Control control) return;
        
        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed) return;

        _isDraggingBottom = true;
        _dragStartY = e.GetPosition(Parent as Visual).Y;
        _dragEndTime = TimeBlock.EndTime;
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void OnBottomHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingBottom || TimeBlock == null || Parent is not Visual parent) return;

        var currentY = e.GetPosition(parent).Y;
        var newEndTime = YToTime(currentY);
        TimeBlock.SetEndTime(newEndTime);
        e.Handled = true;
    }

    private void OnHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingTop || _isDraggingBottom)
        {
            _isDraggingTop = false;
            _isDraggingBottom = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private TimeOnly YToTime(double y)
    {
        var totalMinutes = y / PixelsPerMinute;
        totalMinutes = Math.Clamp(totalMinutes, 0, 24 * 60 - 1);
        return new TimeOnly((int)totalMinutes / 60, (int)totalMinutes % 60);
    }
}
