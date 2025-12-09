using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Presentation.Controls;

public sealed partial class CalendarView : UserControl
{
    public CalendarView()
    {
        this.InitializeComponent();
        SizeChanged += (_, _) => UpdateEventPositions();
        Loaded += (_, _) => UpdateEventPositions();
        DataContextChanged += (_, _) => HookViewModel();
    }

    private void HookViewModel()
    {
        if (Vm != null)
        {
            Vm.VisibleEvents.CollectionChanged += (_, _) => UpdateEventPositions();
            Vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CalendarViewModel.VisibleEvents) ||
                    args.PropertyName == nameof(CalendarViewModel.ViewMode))
                {
                    UpdateEventPositions();
                }
            };
        }
    }

    private CalendarViewModel? Vm => DataContext as CalendarViewModel;

    private void UpdateEventPositions()
    {
        if (Vm == null) return;

        if (WeekEventsItemsControl != null && Vm.IsWeekView)
        {
            var canvasWidth = WeekEventsItemsControl.ActualWidth;
            if (canvasWidth > 0)
            {
                var dayWidth = canvasWidth / 7.0;
                foreach (var eventVm in Vm.VisibleEvents)
                {
                    var left = (eventVm.DayIndex * dayWidth) + 4;
                    var width = Math.Max(20, dayWidth - 8);
                    eventVm.UpdateCanvasPosition(left, width);
                }
            }
        }

        if (DayEventsItemsControl != null && Vm.IsDayView)
        {
            var canvasWidth = DayEventsItemsControl.ActualWidth;
            if (canvasWidth > 0)
            {
                var width = Math.Max(20, canvasWidth - 12);
                foreach (var eventVm in Vm.VisibleEvents)
                {
                    eventVm.UpdateCanvasPosition(4, width);
                }
            }
        }
    }

    private void Event_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: EventViewModel evm })
        {
            evm.SelectCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void Event_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: EventViewModel evm })
        {
            evm.RequestEditCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void DayCell_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: MonthDayViewModel mvm } && Vm != null)
        {
            Vm.RequestNewEventForDateCommand.Execute(mvm.Date);
            e.Handled = true;
        }
    }
}
