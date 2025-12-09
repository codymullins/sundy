using Avalonia;
using Avalonia.Controls;

namespace Sundy.Views;

public partial class CalendarSidebarWindow : Window
{
    private Window? _mainWindow;

    public CalendarSidebarWindow()
    {
        InitializeComponent();
    }

    public void SetMainWindow(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void PositionRelativeToMainWindow()
    {
        if (_mainWindow == null) return;

        var mainPosition = _mainWindow.Position;
        var gap = 10;

        // Position to the left of main window
        var newX = mainPosition.X - (int)Width - gap;
        var newY = mainPosition.Y;

        // Ensure we don't go off the left edge of the screen
        if (newX < 0)
        {
            // Position to the right of main window instead
            newX = mainPosition.X + (int)_mainWindow.Width + gap;
        }

        Position = new PixelPoint(newX, newY);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Hide instead of close, unless app is shutting down
        if (!e.IsProgrammatic)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }
}
