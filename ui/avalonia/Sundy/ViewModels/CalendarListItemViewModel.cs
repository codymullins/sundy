using CommunityToolkit.Mvvm.ComponentModel;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarListItemViewModel(Calendar calendar, Action? onVisibilityChanged = null) : ObservableObject
{
    public string Id => calendar.Id;
    public string Name => calendar.Name;
    public string Color => calendar.Color;

    public bool IsVisible
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                onVisibilityChanged?.Invoke();
            }
        }
    } = true;
}
