using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for individual calendar items with delete functionality.
/// Migrated from Avalonia - uses same CommunityToolkit.Mvvm pattern.
/// </summary>
public partial class CalendarItemViewModel : ObservableObject
{
    private readonly Func<CalendarItemViewModel, CancellationToken, Task> onDeleteRequested;

    public string Id => calendar.Id;
    public string Name => calendar.Name;
    public string Color => calendar.Color;

    public IAsyncRelayCommand DeleteCommand { get; }

    private readonly Calendar calendar;

    public CalendarItemViewModel(
        Calendar calendar,
        Func<CalendarItemViewModel, CancellationToken, Task> onDeleteRequested)
    {
        this.calendar = calendar;
        this.onDeleteRequested = onDeleteRequested;

        DeleteCommand = new AsyncRelayCommand(ct => this.onDeleteRequested(this, ct));
    }
}
