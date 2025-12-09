using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Sundy.Controls;

public class SegmentedControl : TemplatedControl
{
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SegmentedControl, int>(
            nameof(SelectedIndex),
            defaultValue: 0,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private readonly Avalonia.Collections.AvaloniaList<string> _segments = [];

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    [Content]
    public Avalonia.Collections.AvaloniaList<string> Segments
    {
        get => _segments;
    }

    private TranslateTransform? _indicatorTransform;
    private StackPanel? _segmentContainer;
    private Border? _indicator;
    private readonly List<RadioButton> _radioButtons = [];

    public SegmentedControl()
    {
        _segments.CollectionChanged += (_, _) =>
        {
            if (_segmentContainer != null)
            {
                PopulateSegments();
            }
        };
    }

    static SegmentedControl()
    {
        SelectedIndexProperty.Changed.AddClassHandler<SegmentedControl>(
            (x, e) => x.OnSelectedIndexChanged(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _segmentContainer = e.NameScope.Find<StackPanel>("PART_SegmentContainer");
        _indicator = e.NameScope.Find<Border>("PART_Indicator");

        if (_indicator?.RenderTransform is TranslateTransform transform)
        {
            _indicatorTransform = transform;
        }

        if (_segmentContainer != null)
        {
            PopulateSegments();
        }
    }

    private void PopulateSegments()
    {
        if (_segmentContainer == null || Segments.Count == 0)
            return;

        foreach (var button in _radioButtons)
        {
            button.Click -= OnSegmentClicked;
            button.PropertyChanged -= OnButtonBoundsChanged;
        }
        _radioButtons.Clear();
        _segmentContainer.Children.Clear();

        for (var i = 0; i < Segments.Count; i++)
        {
            var radioButton = new RadioButton
            {
                Content = Segments[i],
                GroupName = $"SegmentGroup_{GetHashCode()}",
                IsChecked = i == SelectedIndex,
                Tag = i,
                [!BackgroundProperty] = this[!BackgroundProperty],
            };

            radioButton.Click += OnSegmentClicked;
            radioButton.PropertyChanged += OnButtonBoundsChanged;
            _radioButtons.Add(radioButton);
            _segmentContainer.Children.Add(radioButton);
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateIndicatorPosition(animate: false), 
            Avalonia.Threading.DispatcherPriority.Loaded);
    }

    private void OnButtonBoundsChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            UpdateIndicatorPosition(animate: false);
        }
    }

    private void OnSegmentClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: int index })
        {
            SelectedIndex = index;
        }
    }

    private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is int newIndex)
        {
            for (int i = 0; i < _radioButtons.Count; i++)
            {
                _radioButtons[i].IsChecked = i == newIndex;
            }

            UpdateIndicatorPosition(animate: true);
        }
    }

    private void UpdateIndicatorPosition(bool animate)
    {
        if (_indicatorTransform == null || _segmentContainer == null || _indicator == null)
            return;

        if (SelectedIndex < 0 || SelectedIndex >= _radioButtons.Count)
            return;

        var selectedButton = _radioButtons[SelectedIndex];
        var buttonBounds = selectedButton.Bounds;

        if (buttonBounds.Width <= 0)
            return;

        _indicator.Width = buttonBounds.Width;
        _indicatorTransform.X = buttonBounds.X;
    }
}
