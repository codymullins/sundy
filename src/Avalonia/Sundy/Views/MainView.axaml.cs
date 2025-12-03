using Avalonia.Controls;
using Avalonia.Interactivity;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnOverlayTapped(object? sender, RoutedEventArgs e)
    {
        // Close the dialog when clicking on the overlay
        if (DataContext is MainViewModel vm)
        {
            vm.CloseEventDialogCommand.Execute(null);
        }
    }

    private void OnSettingsOverlayTapped(object? sender, RoutedEventArgs e)
    {
        // Close the settings dialog when clicking on the overlay
        if (DataContext is MainViewModel vm)
        {
            vm.CloseSettingsDialogCommand.Execute(null);
        }
    }

    private void OnDialogContentTapped(object? sender, RoutedEventArgs e)
    {
        // Stop propagation to prevent closing when clicking inside the dialog
        e.Handled = true;
    }
}