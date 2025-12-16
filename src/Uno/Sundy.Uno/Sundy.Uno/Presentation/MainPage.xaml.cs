using System.ComponentModel;
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
        if (args.NewValue is MainViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            UpdateDialogVisibility();
        }

        if (!_initialized && ViewModel != null)
        {
            _ = InitializeViewModelAsync();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsEventDialogOpen) or nameof(MainViewModel.IsSettingsDialogOpen))
        {
            DispatcherQueue.TryEnqueue(UpdateDialogVisibility);
        }
    }

    private void UpdateDialogVisibility()
    {
        EventDialogOverlay.Visibility = ViewModel?.IsEventDialogOpen == true ? Visibility.Visible : Visibility.Collapsed;
        SettingsDialogOverlay.Visibility = ViewModel?.IsSettingsDialogOpen == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
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
