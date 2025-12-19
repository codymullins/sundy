using Microsoft.UI.Xaml.Controls;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Views;

/// <summary>
/// Event edit dialog - create and edit calendar events.
/// Migrated from Avalonia UserControl to WinUI ContentDialog pattern.
/// </summary>
public sealed partial class EventEditView : UserControl
{
    public EventEditViewModel? ViewModel => DataContext as EventEditViewModel;

    public EventEditView()
    {
        this.InitializeComponent();
    }
}
