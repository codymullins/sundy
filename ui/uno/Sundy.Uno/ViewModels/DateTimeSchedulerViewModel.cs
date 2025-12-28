using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for the date and time scheduler popup.
/// </summary>
public partial class DateTimeSchedulerViewModel : ObservableObject
{
    private readonly Action<DateTimeOffset, TimeSpan, TimeSpan>? _onConfirm;
    private readonly Action? _onCancel;

    public DateTimeSchedulerViewModel(
        DateTimeOffset initialDate,
        TimeSpan initialStartTime,
        TimeSpan initialEndTime,
        Action<DateTimeOffset, TimeSpan, TimeSpan>? onConfirm = null,
        Action? onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        SelectedDate = initialDate;
        StartTime = initialStartTime;
        EndTime = initialEndTime;
    }

    [ObservableProperty] private DateTimeOffset _selectedDate;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;

    /// <summary>
    /// Current month display (e.g., "December")
    /// </summary>
    public string CurrentMonth => SelectedDate.DateTime.ToString("MMMM");

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        OnPropertyChanged(nameof(CurrentMonth));
    }

    [RelayCommand]
    private void PreviousWeek()
    {
        SelectedDate = SelectedDate.AddDays(-7);
    }

    [RelayCommand]
    private void NextWeek()
    {
        SelectedDate = SelectedDate.AddDays(7);
    }

    [RelayCommand]
    private void Confirm()
    {
        _onConfirm?.Invoke(SelectedDate, StartTime, EndTime);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel?.Invoke();
    }
}
