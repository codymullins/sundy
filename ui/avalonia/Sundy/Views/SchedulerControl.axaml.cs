using Avalonia.Controls;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Views;

public partial class SchedulerControl : UserControl
{
    public SchedulerControl()
    {
        InitializeComponent();
    }
    
    public SchedulerControl(SchedulerViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}

