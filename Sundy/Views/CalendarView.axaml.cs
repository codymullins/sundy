using Avalonia.Controls;
using Avalonia.Input;
using Sundy.ViewModels;

namespace Sundy.Views;

public partial class CalendarView : UserControl
{
    public CalendarView()
    {
        InitializeComponent();
    }

    private void OnEventDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: EventViewModel eventVm })
        {
            eventVm.RequestEditCommand.Execute(null);
            e.Handled = true;
        }
    }
}