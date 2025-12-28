using System;

#if MACOS_DESKTOP
using Sundy.Uno.Platforms.Desktop.MacOS;
#endif

namespace Sundy.Uno.Services;

/// <summary>
/// Cross-platform implementation of the menu bar service.
/// On macOS, displays a status bar item with the current day number.
/// On other platforms, this is a no-op.
/// </summary>
public class MenuBarService : IMenuBarService
{
#if MACOS_DESKTOP
    private StatusBarController? _statusBarController;
#endif

    public event Action<DateTime>? DateClicked;

    public void Initialize()
    {
#if MACOS_DESKTOP
        if (!OperatingSystem.IsMacOS()) return;

        _statusBarController = new StatusBarController();
        _statusBarController.DateClicked += OnDateClicked;
        _statusBarController.Initialize();
#endif
    }

    public void UpdateNextEvent(string? eventTitle, DateTime? eventTime)
    {
#if MACOS_DESKTOP
        _statusBarController?.UpdateNextEvent(eventTitle, eventTime);
#endif
    }

    public void UpdateDisplayedMonth(int year, int month)
    {
#if MACOS_DESKTOP
        _statusBarController?.UpdateDisplayedMonth(year, month);
#endif
    }

    public void Shutdown()
    {
#if MACOS_DESKTOP
        if (_statusBarController != null)
        {
            _statusBarController.DateClicked -= OnDateClicked;
            _statusBarController.Shutdown();
            _statusBarController = null;
        }
#endif
    }

#if MACOS_DESKTOP
    private void OnDateClicked(DateTime date)
    {
        DateClicked?.Invoke(date);
    }
#endif
}
