using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using Windows.Foundation;

namespace Sundy.Uno.Presentation.Controls;

[ContentProperty(Name = nameof(Segments))]
public class SegmentedControl : Control
{
    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(SegmentedControl),
            new PropertyMetadata(0, OnSelectedIndexChanged));

    private readonly ObservableCollection<string> _segments = new();
    private StackPanel? _segmentContainer;
    private Border? _indicator;
    private readonly List<RadioButton> _radioButtons = new();
    private bool _isUpdatingSelection;
    
    // Composition animation fields
    private Compositor? _compositor;
    private Visual? _indicatorVisual;

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public ObservableCollection<string> Segments => _segments;

    public SegmentedControl()
    {
        DefaultStyleKey = typeof(SegmentedControl);

        _segments.CollectionChanged += OnSegmentsCollectionChanged;
    }

    private void OnSegmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_segmentContainer != null)
        {
            PopulateSegments();
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _segmentContainer = GetTemplateChild("PART_SegmentContainer") as StackPanel;
        _indicator = GetTemplateChild("PART_Indicator") as Border;

        if (_indicator != null)
        {
            _indicatorVisual = ElementCompositionPreview.GetElementVisual(_indicator);
            _compositor = _indicatorVisual.Compositor;
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

        // Clean up existing buttons
        foreach (var button in _radioButtons)
        {
            button.Click -= OnSegmentClicked;
            button.SizeChanged -= OnButtonSizeChanged;
        }
        _radioButtons.Clear();
        _segmentContainer.Children.Clear();

        // Create new buttons for each segment
        for (var i = 0; i < Segments.Count; i++)
        {
            var radioButton = new RadioButton
            {
                Content = Segments[i],
                GroupName = $"SegmentGroup_{GetHashCode()}",
                IsChecked = i == SelectedIndex,
                Tag = i,
                Style = Application.Current.Resources["SegmentedControlRadioButtonStyle"] as Style
            };

            radioButton.Click += OnSegmentClicked;
            radioButton.SizeChanged += OnButtonSizeChanged;
            _radioButtons.Add(radioButton);
            _segmentContainer.Children.Add(radioButton);
        }

        // Defer initial positioning until layout is complete
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            UpdateIndicatorPosition(animate: false);
        });
    }

    private void OnButtonSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Recalculate position when button size changes (e.g., window resize)
        // Don't animate on resize
        UpdateIndicatorPosition(animate: false);
    }

    private void OnSegmentClicked(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingSelection)
            return;

        if (sender is RadioButton { Tag: int index })
        {
            SelectedIndex = index;
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedControl control && e.NewValue is int newIndex)
        {
            control.OnSelectedIndexChangedCore(newIndex);
        }
    }

    private void OnSelectedIndexChangedCore(int newIndex)
    {
        // Prevent re-entrancy
        _isUpdatingSelection = true;
        try
        {
            // Update all radio buttons' checked state
            for (int i = 0; i < _radioButtons.Count; i++)
            {
                _radioButtons[i].IsChecked = i == newIndex;
            }

            UpdateIndicatorPosition(animate: true);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private void UpdateIndicatorPosition(bool animate)
    {
        if (_segmentContainer == null || _indicator == null)
            return;

        if (SelectedIndex < 0 || SelectedIndex >= _radioButtons.Count)
            return;

        var selectedButton = _radioButtons[SelectedIndex];

        // Wait for layout if button doesn't have size yet
        if (selectedButton.ActualWidth <= 0)
            return;

        // Calculate target position relative to container
        try
        {
            var position = selectedButton.TransformToVisual(_segmentContainer)
                                        .TransformPoint(new Point(0, 0));

            var targetWidth = selectedButton.ActualWidth;
            var targetX = position.X;

            if (targetWidth <= 0)
                return;

            if (animate && _compositor != null && _indicatorVisual != null)
            {
                AnimateIndicator(targetWidth, targetX);
            }
            else
            {
                // Set directly without animation
                _indicator.Width = targetWidth;
                if (_indicatorVisual != null)
                {
                    _indicatorVisual.Offset = new Vector3((float)targetX, 0, 0);
                }
            }
        }
        catch
        {
            // TransformToVisual can throw if visual tree is not ready
            // Silently ignore and wait for next layout pass
        }
    }

    private void AnimateIndicator(double targetWidth, double targetX)
    {
        if (_indicator == null || _compositor == null || _indicatorVisual == null)
            return;

        var duration = TimeSpan.FromMilliseconds(150);
        var easing = _compositor.CreateCubicBezierEasingFunction(
            new Vector2(0.25f, 0.1f),
            new Vector2(0.25f, 1f)); // Cubic ease out

        // Animate offset (position)
        var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(1f, new Vector3((float)targetX, 0, 0), easing);
        offsetAnimation.Duration = duration;

        // Animate size (scale-based approach for width)
        // We need to set the actual width and animate the visual's scale
        var currentWidth = double.IsNaN(_indicator.Width) || _indicator.Width <= 0 
            ? targetWidth 
            : _indicator.Width;
        
        // Set target width immediately (composition will handle the visual smoothly)
        _indicator.Width = targetWidth;

        // Start the offset animation
        _indicatorVisual.StartAnimation("Offset", offsetAnimation);

        // For width animation, we use a scale animation relative to the new width
        if (Math.Abs(currentWidth - targetWidth) > 0.1)
        {
            var scaleFrom = (float)(currentWidth / targetWidth);
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new Vector3(scaleFrom, 1f, 1f));
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f), easing);
            scaleAnimation.Duration = duration;
            
            // Set transform origin to left edge
            _indicatorVisual.CenterPoint = new Vector3(0, (float)(_indicator.ActualHeight / 2), 0);
            _indicatorVisual.StartAnimation("Scale", scaleAnimation);
        }
    }
}
