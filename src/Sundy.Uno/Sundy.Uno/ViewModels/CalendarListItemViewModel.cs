using CommunityToolkit.Mvvm.ComponentModel;
using Sundy.Core;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for calendar list items with visibility toggle.
/// Migrated from Avalonia - uses same CommunityToolkit.Mvvm pattern.
/// </summary>
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
