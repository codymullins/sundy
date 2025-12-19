using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Views;

/// <summary>
/// Calendar view - shows month/week/day views with events.
/// Migrated from Avalonia - preserving layout and event handling.
/// </summary>
public sealed partial class CalendarView : UserControl
{
    public CalendarView()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    public CalendarViewModel? ViewModel => DataContext as CalendarViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(CalendarViewModel.MonthDays))
                {
                    BuildMonthGrid();
                }
            };
            BuildMonthGrid();
        }
    }

    private void BuildMonthGrid()
    {
        if (ViewModel?.MonthDays == null) return;

        MonthDaysContainer.Children.Clear();

        foreach (var dayVm in ViewModel.MonthDays)
        {
            var dayCell = CreateDayCell(dayVm);
            Grid.SetRow(dayCell, dayVm.GridRow);
            Grid.SetColumn(dayCell, dayVm.GridColumn);
            MonthDaysContainer.Children.Add(dayCell);
        }
    }

    private Border CreateDayCell(MonthDayViewModel dayVm)
    {
        var border = new Border
        {
            BorderBrush = (Brush)Resources["BorderBrush"] ?? Application.Current.Resources["BorderBrush"] as Brush,
            BorderThickness = new Thickness(0, 0, 1, 1),
            Background = (Brush)Resources["BackgroundPrimary"] ?? Application.Current.Resources["BackgroundPrimary"] as Brush
        };
        border.DoubleTapped += OnDayDoubleTapped;
        border.DataContext = dayVm;

        var contentGrid = new Grid { Padding = new Thickness(5) };
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Day number
        var dayNumberBorder = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 5, 5),
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24,
            Background = dayVm.IsToday
                ? (Brush)Application.Current.Resources["AccentPrimary"]
                : new SolidColorBrush(Colors.Transparent)
        };

        var dayText = new TextBlock
        {
            Text = dayVm.Day,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = dayVm.IsToday
                ? new SolidColorBrush(Colors.White)
                : (Brush)Application.Current.Resources["ForegroundPrimary"],
            FontWeight = dayVm.IsToday
                ? Microsoft.UI.Text.FontWeights.Bold
                : Microsoft.UI.Text.FontWeights.Normal
        };
        dayNumberBorder.Child = dayText;
        Grid.SetRow(dayNumberBorder, 0);
        contentGrid.Children.Add(dayNumberBorder);

        // Events list
        var eventsRepeater = new ItemsRepeater
        {
            ItemsSource = dayVm.Events,
            Margin = new Thickness(0, 4, 0, 0),
            ItemTemplate = (DataTemplate)Application.Current.Resources["EventItemTemplate"]
                ?? CreateEventTemplate()
        };
        Grid.SetRow(eventsRepeater, 1);
        contentGrid.Children.Add(eventsRepeater);

        border.Child = contentGrid;
        return border;
    }

    private DataTemplate CreateEventTemplate()
    {
        // Fallback if template not in resources - events will use simple text
        return null!;
    }

    private void OnDayDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is MonthDayViewModel dayVm && ViewModel != null)
        {
            ViewModel.RequestNewEventForDateCommand.Execute(dayVm.Date);
        }
    }

    private void OnEventTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is EventViewModel eventVm)
        {
            ViewModel?.RequestEditEvent(eventVm);
        }
    }
}
