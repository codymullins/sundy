using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Sundy.Uno.Services;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno;

/// <summary>
/// Main page for the Sundy calendar application.
/// Migrated from Avalonia MainView.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI
        ViewModel = App.Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;
        
        // Set XamlRoot for ContentDialog support
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        
        // Initialize asynchronously
        _ = ViewModel.InitializeAsync();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var xamlRootProvider = App.Services.GetRequiredService<IXamlRootProvider>();
        xamlRootProvider.XamlRoot = this.XamlRoot;
        ViewModel.CurrentWindowWidth = ActualWidth;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.CurrentWindowWidth = e.NewSize.Width;
    }

    private void OnSidebarBackdropTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.CloseSidebarCommand.Execute(null);
    }

    private void OnSidebarContentTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }
}
