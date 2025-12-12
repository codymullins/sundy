using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Sundy.Uno.Presentation.Controls;

public class PixelSnappingBorder : Border
{
    public static readonly DependencyProperty SnapMultipleProperty =
        DependencyProperty.Register(
            nameof(SnapMultiple),
            typeof(int),
            typeof(PixelSnappingBorder),
            new PropertyMetadata(7));

    public int SnapMultiple
    {
        get => (int)GetValue(SnapMultipleProperty);
        set => SetValue(SnapMultipleProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var snapped = SnapSize(availableSize);
        return base.MeasureOverride(snapped);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var snapped = SnapSize(finalSize);
        return base.ArrangeOverride(snapped);
    }

    private Size SnapSize(Size size)
    {
        var multiple = SnapMultiple;
        if (multiple <= 0) multiple = 7;

        var snappedWidth = double.IsInfinity(size.Width) || double.IsNaN(size.Width)
            ? size.Width
            : Math.Floor(size.Width / multiple) * multiple;

        return new Size(snappedWidth, size.Height);
    }
}
