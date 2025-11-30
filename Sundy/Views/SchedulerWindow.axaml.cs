using Avalonia.Controls;
using Sundy.ViewModels.Scheduler;

namespace Sundy.Views;

public partial class SchedulerWindow : Window
{
    public SchedulerWindow()
    {
        InitializeComponent();
    }
    
    public SchedulerWindow(SchedulerViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}

