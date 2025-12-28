using Microsoft.UI.Xaml.Controls;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Views;

/// <summary>
/// Calendar settings dialog - manage calendars, database, and integrations.
/// Migrated from Avalonia UserControl to WinUI ContentDialog pattern.
/// </summary>
public sealed partial class CalendarSettingsView : UserControl
{
    public CalendarSettingsViewModel? ViewModel => DataContext as CalendarSettingsViewModel;

    public CalendarSettingsView()
    {
        this.InitializeComponent();
    }
}
