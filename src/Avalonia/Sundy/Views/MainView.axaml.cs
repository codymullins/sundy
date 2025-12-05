using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // Subscribe to bounds changes for responsive behavior
        this.PropertyChanged += (sender, e) =>
        {
            if (e.Property == BoundsProperty && DataContext is MainViewModel vm)
            {
                vm.CurrentWindowWidth = Bounds.Width;
            }
        };
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

    private void OnSidebarBackdropTapped(object? sender, RoutedEventArgs e)
    {
        // Close sidebar when tapping backdrop (mobile only)
        if (DataContext is MainViewModel vm)
        {
            vm.CloseSidebarCommand.Execute(null);
        }
    }

    private void OnSidebarContentTapped(object? sender, RoutedEventArgs e)
    {
        // Prevent tap from propagating to backdrop
        e.Handled = true;
    }
}