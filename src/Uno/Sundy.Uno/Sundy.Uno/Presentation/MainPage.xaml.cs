using Microsoft.Extensions.DependencyInjection;
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
        var app = (App?)Application.Current;
        var services = app?.Host?.Services;
        if (services != null && DataContext == null)
        {
            var vm = services.GetRequiredService<MainViewModel>();
            DataContext = vm;
        }

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        if (ViewModel != null)
        {
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
    }
}
