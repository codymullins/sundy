using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Sundy.Uno.Services;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno;

/// <summary>
/// Main page for the Sundy calendar application.
/// Migrated from Avalonia MainView.
/// </summary>
public sealed partial class MainPage : Page
{
    private const double SidebarAnimationDuration = 200;
    private const double DesktopSidebarWidth = 266;
    private const double MobileSidebarWidth = 292;

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

        // Subscribe to sidebar state changes for animations
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Initialize asynchronously
        _ = ViewModel.InitializeAsync();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var xamlRootProvider = App.Services.GetRequiredService<IXamlRootProvider>();
        xamlRootProvider.XamlRoot = this.XamlRoot;
        ViewModel.CurrentWindowWidth = ActualWidth;

        // Set initial sidebar states
        UpdateDesktopSidebarState(ViewModel.IsDesktopSidebarVisible, animate: false);
        UpdateMobileSidebarState(ViewModel.IsSidebarOpen, animate: false);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.CurrentWindowWidth = e.NewSize.Width;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsDesktopSidebarVisible))
        {
            UpdateDesktopSidebarState(ViewModel.IsDesktopSidebarVisible, animate: true);
        }
        else if (e.PropertyName == nameof(MainViewModel.IsSidebarOpen))
        {
            UpdateMobileSidebarState(ViewModel.IsSidebarOpen, animate: true);
        }
    }

    private void UpdateDesktopSidebarState(bool isVisible, bool animate)
    {
        var targetX = isVisible ? 0 : -DesktopSidebarWidth;

        if (animate)
        {
            AnimateTranslateX(DesktopSidebarTranslate, targetX);
        }
        else
        {
            DesktopSidebarTranslate.X = targetX;
        }
    }

    private void UpdateMobileSidebarState(bool isOpen, bool animate)
    {
        var targetX = isOpen ? 0 : -MobileSidebarWidth;
        var targetOpacity = isOpen ? 1.0 : 0.0;

        if (animate)
        {
            AnimateTranslateX(MobileSidebarTranslate, targetX);
            AnimateOpacity(MobileSidebarBackdrop, targetOpacity, isOpen);
        }
        else
        {
            MobileSidebarTranslate.X = targetX;
            MobileSidebarBackdrop.Opacity = targetOpacity;
            MobileSidebarBackdrop.IsHitTestVisible = isOpen;
        }
    }

    private void AnimateTranslateX(Microsoft.UI.Xaml.Media.TranslateTransform transform, double targetX)
    {
        var animation = new DoubleAnimation
        {
            To = targetX,
            Duration = new Duration(TimeSpan.FromMilliseconds(SidebarAnimationDuration)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, transform);
        Storyboard.SetTargetProperty(animation, "X");
        storyboard.Begin();
    }

    private void AnimateOpacity(UIElement element, double targetOpacity, bool enableHitTest)
    {
        // Enable hit test immediately when opening, disable after closing
        if (enableHitTest)
        {
            MobileSidebarBackdrop.IsHitTestVisible = true;
        }

        var animation = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = new Duration(TimeSpan.FromMilliseconds(SidebarAnimationDuration)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "Opacity");

        if (!enableHitTest)
        {
            storyboard.Completed += (_, _) => MobileSidebarBackdrop.IsHitTestVisible = false;
        }

        storyboard.Begin();
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
