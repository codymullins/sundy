using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for individual days in month calendar view.
/// Migrated from Avalonia - same logic preserved.
/// </summary>
public partial class MonthDayViewModel : ObservableObject
{
    private readonly Action<MonthDayViewModel, EventViewModel>? _onEventSelected;

    public MonthDayViewModel(DateTime date, bool isCurrentMonth, bool isToday, List<EventViewModel> events, int gridRow, int gridColumn, Action<MonthDayViewModel, EventViewModel>? onEventSelected = null)
    {
        Date = date;
        Day = date.Day.ToString();
        IsCurrentMonth = isCurrentMonth;
        IsToday = isToday;
        Events = new ObservableCollection<EventViewModel>(events);
        GridRow = gridRow;
        GridColumn = gridColumn;
        _onEventSelected = onEventSelected;
    }

    public DateTime Date { get; }
    public string Day { get; }
    public bool IsCurrentMonth { get; }
    public bool IsToday { get; }
    public ObservableCollection<EventViewModel> Events { get; }
    public int GridRow { get; }
    public int GridColumn { get; }

    [ObservableProperty]
    private EventViewModel? _selectedEvent;

    partial void OnSelectedEventChanged(EventViewModel? value)
    {
        if (value != null && _onEventSelected != null)
            _onEventSelected(this, value);
    }

    public string? Background => IsCurrentMonth ? null : "#00000000";
    public string? DayForeground => null;
}
