using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private CompositeTransform? _indicatorTransform;
    private StackPanel? _segmentContainer;
    private Border? _indicator;
    private readonly List<RadioButton> _radioButtons = new();
    private Storyboard? _currentStoryboard;
    private bool _isUpdatingSelection;

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
        _indicatorTransform = GetTemplateChild("PART_IndicatorTransform") as CompositeTransform;

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
        if (_indicator == null || _indicatorTransform == null || _segmentContainer == null)
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

            if (animate)
            {
                AnimateIndicator(targetWidth, targetX);
            }
            else
            {
                // Set directly without animation
                _indicator.Width = targetWidth;
                _indicatorTransform.TranslateX = targetX;
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
        if (_indicator == null || _indicatorTransform == null)
            return;

        // Stop any current animation and preserve the current values
        if (_currentStoryboard != null)
        {
            _currentStoryboard.Stop();
            _currentStoryboard = null;
        }

        // Get current values for smooth animation from current position
        var currentWidth = _indicator.Width;
        var currentX = _indicatorTransform.TranslateX;

        // Handle NaN or invalid values
        if (double.IsNaN(currentWidth) || currentWidth <= 0)
            currentWidth = targetWidth;
        if (double.IsNaN(currentX))
            currentX = targetX;

        var storyboard = new Storyboard();

        // Animate Width - requires EnableDependentAnimation for non-independent properties
        var widthAnimation = new DoubleAnimation
        {
            From = currentWidth,
            To = targetWidth,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
            EnableDependentAnimation = true
        };
        Storyboard.SetTarget(widthAnimation, _indicator);
        Storyboard.SetTargetProperty(widthAnimation, "Width");

        // Animate TranslateX - this is an independent animation (GPU accelerated)
        var translateAnimation = new DoubleAnimation
        {
            From = currentX,
            To = targetX,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(translateAnimation, _indicatorTransform);
        Storyboard.SetTargetProperty(translateAnimation, "TranslateX");

        storyboard.Children.Add(widthAnimation);
        storyboard.Children.Add(translateAnimation);

        // Ensure final values are applied after animation completes
        storyboard.Completed += (s, e) =>
        {
            _indicator.Width = targetWidth;
            _indicatorTransform.TranslateX = targetX;
        };

        _currentStoryboard = storyboard;
        storyboard.Begin();
    }
}
