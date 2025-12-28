using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.ViewModels;

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
