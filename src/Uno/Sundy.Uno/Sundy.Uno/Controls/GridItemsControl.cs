using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno.Controls;

public class GridItemsControl : ItemsControl
{
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (item is MonthDayViewModel dayViewModel && element is FrameworkElement container)
        {
            Grid.SetRow(container, dayViewModel.GridRow);
            Grid.SetColumn(container, dayViewModel.GridColumn);
        }
    }
}
