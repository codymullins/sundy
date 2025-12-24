using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Sundy.Uno.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace Sundy.Uno.Views;

/// <summary>
/// Date and time scheduler view with smooth pointer-based dragging.
/// </summary>
public sealed partial class DateTimeSchedulerView : UserControl
{
    private const double HourHeight = 60.0;
    private const double MinBlockHeight = 30.0; // Minimum 30 minutes
    private const double SnapInterval = 15.0; // Snap to 15-minute intervals

    // Drag state
    private bool _isDraggingBlock;
    private bool _isDraggingTopHandle;
    private bool _isDraggingBottomHandle;
    private double _dragStartY;
    private double _blockStartTop;
    private double _blockStartHeight;
    private uint _capturedPointerId;

    // Current time block position
    private double _blockTop;
    private double _blockHeight;

    // Week days
    private DateTimeOffset[] _weekDays = new DateTimeOffset[7];
    private int _selectedDayIndex;

    // UI element references
    private readonly TextBlock[] _dayNumbers;
    private readonly Ellipse[] _daySelectedIndicators;

    public DateTimeSchedulerViewModel? ViewModel => DataContext as DateTimeSchedulerViewModel;

    public DateTimeSchedulerView()
    {
        this.InitializeComponent();

        // Store references to day elements for fast updates
        _dayNumbers = new[] { Day0Number, Day1Number, Day2Number, Day3Number, Day4Number, Day5Number, Day6Number };
        _daySelectedIndicators = new[] { Day0Selected, Day1Selected, Day2Selected, Day3Selected, Day4Selected, Day5Selected, Day6Selected };

        this.Loaded += OnLoaded;
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeFromViewModel();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DateTimeSchedulerViewModel.SelectedDate))
        {
            UpdateWeekDays();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            InitializeFromViewModel();
        }
    }

    private void InitializeFromViewModel()
    {
        if (ViewModel == null) return;

        // Initialize time block position from ViewModel
        _blockTop = ViewModel.StartTime.TotalHours * HourHeight;
        var duration = ViewModel.EndTime - ViewModel.StartTime;
        if (duration.TotalMinutes < 0) duration = duration.Add(TimeSpan.FromHours(24));
        _blockHeight = Math.Max(duration.TotalHours * HourHeight, MinBlockHeight);

        UpdateTimeBlockPosition();
        UpdateTimeBlockText();
        UpdateWeekDays();

        // Scroll to show the time block
        var scrollPosition = Math.Max(0, _blockTop - 80);
        TimeScrollViewer.ChangeView(null, scrollPosition, null, true);
    }

    private void UpdateWeekDays()
    {
        if (ViewModel == null) return;

        var selectedDate = ViewModel.SelectedDate;
        var weekStart = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);

        for (int i = 0; i < 7; i++)
        {
            _weekDays[i] = weekStart.AddDays(i);
            _dayNumbers[i].Text = _weekDays[i].Day.ToString();

            var isSelected = _weekDays[i].Date == selectedDate.Date;
            _daySelectedIndicators[i].Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;

            if (isSelected)
            {
                _selectedDayIndex = i;
            }
        }
    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tagStr && int.TryParse(tagStr, out int dayIndex))
        {
            if (dayIndex >= 0 && dayIndex < 7)
            {
                // Update selection
                _daySelectedIndicators[_selectedDayIndex].Visibility = Visibility.Collapsed;
                _daySelectedIndicators[dayIndex].Visibility = Visibility.Visible;
                _selectedDayIndex = dayIndex;

                // Update ViewModel
                if (ViewModel != null)
                {
                    ViewModel.SelectedDate = _weekDays[dayIndex];
                }
            }
        }
    }

    #region Pointer-based Dragging

    private void TimeGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(TimeGridContainer);
        var position = point.Position;

        // Check if clicking on the time block area
        var blockLeft = 65 + 8; // Time column width + margin
        var blockRight = TimeGridContainer.ActualWidth - 16;
        var blockBottom = _blockTop + _blockHeight;

        if (position.X >= blockLeft && position.X <= blockRight)
        {
            // Check for top handle (within 20px of top edge)
            if (position.Y >= _blockTop - 10 && position.Y <= _blockTop + 20)
            {
                StartTopHandleDrag(e, position.Y);
                return;
            }

            // Check for bottom handle (within 20px of bottom edge)
            if (position.Y >= blockBottom - 20 && position.Y <= blockBottom + 10)
            {
                StartBottomHandleDrag(e, position.Y);
                return;
            }

            // Check for block body drag
            if (position.Y >= _blockTop && position.Y <= blockBottom)
            {
                StartBlockDrag(e, position.Y);
                return;
            }
        }
    }

    private void StartBlockDrag(PointerRoutedEventArgs e, double y)
    {
        _isDraggingBlock = true;
        _dragStartY = y;
        _blockStartTop = _blockTop;
        _capturedPointerId = e.Pointer.PointerId;
        TimeGridContainer.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void StartTopHandleDrag(PointerRoutedEventArgs e, double y)
    {
        _isDraggingTopHandle = true;
        _dragStartY = y;
        _blockStartTop = _blockTop;
        _blockStartHeight = _blockHeight;
        _capturedPointerId = e.Pointer.PointerId;
        TimeGridContainer.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void StartBottomHandleDrag(PointerRoutedEventArgs e, double y)
    {
        _isDraggingBottomHandle = true;
        _dragStartY = y;
        _blockStartHeight = _blockHeight;
        _capturedPointerId = e.Pointer.PointerId;
        TimeGridContainer.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void TimeGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDraggingBlock && !_isDraggingTopHandle && !_isDraggingBottomHandle)
            return;

        var point = e.GetCurrentPoint(TimeGridContainer);
        var deltaY = point.Position.Y - _dragStartY;

        if (_isDraggingBlock)
        {
            // Move the entire block
            var newTop = _blockStartTop + deltaY;
            newTop = SnapToInterval(newTop);
            newTop = Math.Clamp(newTop, 0, 24 * HourHeight - _blockHeight);

            _blockTop = newTop;
            UpdateTimeBlockPosition();
            UpdateTimeBlockText();
        }
        else if (_isDraggingTopHandle)
        {
            // Resize from top (changes start time)
            var newTop = _blockStartTop + deltaY;
            newTop = SnapToInterval(newTop);
            var blockBottom = _blockStartTop + _blockStartHeight;
            var newHeight = blockBottom - newTop;

            if (newTop >= 0 && newHeight >= MinBlockHeight)
            {
                _blockTop = newTop;
                _blockHeight = newHeight;
                UpdateTimeBlockPosition();
                UpdateTimeBlockText();
            }
        }
        else if (_isDraggingBottomHandle)
        {
            // Resize from bottom (changes end time)
            var newHeight = _blockStartHeight + deltaY;
            newHeight = SnapToInterval(newHeight);
            newHeight = Math.Max(newHeight, MinBlockHeight);

            var maxHeight = 24 * HourHeight - _blockTop;
            newHeight = Math.Min(newHeight, maxHeight);

            _blockHeight = newHeight;
            UpdateTimeBlockPosition();
            UpdateTimeBlockText();
        }

        e.Handled = true;
    }

    private void TimeGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        EndDrag(e);
    }

    private void TimeGrid_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        EndDrag(e);
    }

    private void EndDrag(PointerRoutedEventArgs e)
    {
        if (!_isDraggingBlock && !_isDraggingTopHandle && !_isDraggingBottomHandle)
            return;

        _isDraggingBlock = false;
        _isDraggingTopHandle = false;
        _isDraggingBottomHandle = false;

        TimeGridContainer.ReleasePointerCapture(e.Pointer);

        // Update ViewModel with final values
        if (ViewModel != null)
        {
            var startHours = _blockTop / HourHeight;
            var endHours = (_blockTop + _blockHeight) / HourHeight;

            ViewModel.StartTime = TimeSpan.FromHours(startHours);
            ViewModel.EndTime = TimeSpan.FromHours(Math.Min(endHours, 24));
        }

        e.Handled = true;
    }

    #endregion

    #region UI Updates

    private void UpdateTimeBlockPosition()
    {
        Canvas.SetTop(TimeBlock, _blockTop);
        Canvas.SetLeft(TimeBlock, 0);
        TimeBlock.Width = TimeBlockCanvas.ActualWidth > 0 ? TimeBlockCanvas.ActualWidth : 280;
        TimeBlock.Height = _blockHeight;
    }

    private void UpdateTimeBlockText()
    {
        var startHours = _blockTop / HourHeight;
        var endHours = (_blockTop + _blockHeight) / HourHeight;

        var startTime = TimeSpan.FromHours(startHours);
        var endTime = TimeSpan.FromHours(endHours);

        var startDateTime = DateTime.Today.Add(startTime);
        var endDateTime = DateTime.Today.Add(endTime);

        TimeRangeText.Text = $"{startDateTime:h:mm tt} → {endDateTime:h:mm tt}";

        var duration = endTime - startTime;
        if (duration.TotalMinutes < 60)
        {
            DurationText.Text = $"{(int)duration.TotalMinutes} min";
        }
        else
        {
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            DurationText.Text = minutes == 0 ? $"{hours} hr" : $"{hours} hr {minutes} min";
        }
    }

    private double SnapToInterval(double value)
    {
        // Snap to 15-minute intervals (15 minutes = 0.25 hours = 15 pixels)
        var pixelsPerInterval = (SnapInterval / 60.0) * HourHeight;
        return Math.Round(value / pixelsPerInterval) * pixelsPerInterval;
    }

    #endregion
}
