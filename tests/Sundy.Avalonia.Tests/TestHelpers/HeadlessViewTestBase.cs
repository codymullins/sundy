using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Mediator;
using Moq;
using Sundy.Core;
using Sundy.Core.Queries;
using Sundy.Test.TestHelpers;

namespace Sundy.Avalonia.Test.TestHelpers;

public abstract class HeadlessViewTestBase
{
    /// <summary>
    /// Creates a test window hosting the provided control.
    /// The window is shown and layout is flushed before returning.
    /// </summary>
    protected Window CreateTestWindow(Control content)
    {
        var window = new Window
        {
            Content = content,
            Width = 800,
            Height = 600
        };
        window.Show();
        Dispatcher.UIThread.RunJobs(); // Process pending layout/render/binding updates
        return window;
    }

    /// <summary>
    /// Creates a mock IMediator with default calendar query setup.
    /// </summary>
    protected Mock<IMediator> CreateMediatorMock(List<Core.Calendar>? calendars = null)
    {
        calendars ??= new List<Core.Calendar> { TestDataBuilder.CreateTestCalendar() };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllCalendarsQuery>(), default))
            .ReturnsAsync(calendars);

        return mediatorMock;
    }

    /// <summary>
    /// Finds a control by its x:Name attribute in the visual tree.
    /// </summary>
    protected T? FindControlByName<T>(Control root, string name) where T : Control
    {
        return root.FindControl<T>(name);
    }

    /// <summary>
    /// Finds a control by type in the visual tree.
    /// </summary>
    protected T? FindControlByType<T>(Visual root) where T : Control
    {
        return root.GetVisualDescendants().OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Finds all controls of a specific type in the visual tree.
    /// </summary>
    protected IEnumerable<T> FindAllControlsByType<T>(Visual root) where T : Control
    {
        return root.GetVisualDescendants().OfType<T>();
    }

    /// <summary>
    /// Simulates a button click by raising the Click event.
    /// Automatically flushes the dispatcher queue after the click.
    /// </summary>
    protected void SimulateClick(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }
}

/// <summary>
/// Extension methods for visual tree navigation.
/// </summary>
public static class VisualTreeExtensions
{
    public static IEnumerable<Visual> GetVisualDescendants(this Visual visual)
    {
        foreach (var child in visual.GetVisualChildren())
        {
            yield return child;
            foreach (var descendant in child.GetVisualDescendants())
            {
                yield return descendant;
            }
        }
    }
}
