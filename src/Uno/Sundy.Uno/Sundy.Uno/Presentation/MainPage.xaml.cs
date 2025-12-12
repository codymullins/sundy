using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Presentation;

public sealed partial class MainPage : Page
{
    public MainViewModel? ViewModel => DataContext as MainViewModel;
    private bool _initialized;

    public MainPage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // ViewModel is set by Uno Navigation framework after construction
        if (ViewModel != null && !_initialized)
        {
            _ = InitializeViewModelAsync();
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Fallback: initialize if DataContextChanged fired before Loaded
        if (!_initialized && ViewModel != null)
        {
            await InitializeViewModelAsync();
        }
    }

    private async Task InitializeViewModelAsync()
    {
        if (_initialized) return;
        _initialized = true;
        await ViewModel!.InitializeAsync();
    }
}
