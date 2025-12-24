#if MACOS_DESKTOP
using System;
using System.Runtime.InteropServices;

namespace Sundy.Uno.Platforms.Desktop.MacOS;

/// <summary>
/// Controls the macOS menu bar status item using Objective-C runtime interop.
/// Shows the current day number and a dropdown menu with a month minimap.
/// </summary>
public class StatusBarController : IDisposable
{
    private IntPtr _statusItem;
    private IntPtr _menu;
    private MenuBarCalendarView? _calendarView;
    private bool _disposed;
    private DateTime _displayedMonth;

    // Cached selectors
    private static readonly IntPtr SystemStatusBarSel = ObjCInterop.RegisterSelector("systemStatusBar");
    private static readonly IntPtr StatusItemWithLengthSel = ObjCInterop.RegisterSelector("statusItemWithLength:");
    private static readonly IntPtr ButtonSel = ObjCInterop.RegisterSelector("button");
    private static readonly IntPtr SetTitleSel = ObjCInterop.RegisterSelector("setTitle:");
    private static readonly IntPtr SetMenuSel = ObjCInterop.RegisterSelector("setMenu:");
    private static readonly IntPtr RemoveStatusItemSel = ObjCInterop.RegisterSelector("removeStatusItem:");
    private static readonly IntPtr AddItemWithTitleActionKeyEquivalentSel = ObjCInterop.RegisterSelector("addItemWithTitle:action:keyEquivalent:");
    private static readonly IntPtr AddItemSel = ObjCInterop.RegisterSelector("addItem:");
    private static readonly IntPtr SeparatorItemSel = ObjCInterop.RegisterSelector("separatorItem");
    private static readonly IntPtr SetToolTipSel = ObjCInterop.RegisterSelector("setToolTip:");
    private static readonly IntPtr SetVisibleSel = ObjCInterop.RegisterSelector("setVisible:");
    private static readonly IntPtr SetImageSel = ObjCInterop.RegisterSelector("setImage:");
    private static readonly IntPtr SetViewSel = ObjCInterop.RegisterSelector("setView:");

    // NSStatusItem length constant
    private const double NSVariableStatusItemLength = -1.0;

    public event Action<DateTime>? DateClicked;

    public void Initialize()
    {
        try
        {
            Console.WriteLine("[StatusBar] Starting initialization...");

            // Ensure AppKit is loaded
            ObjCInterop.EnsureAppKitLoaded();

            // Get NSApplication.sharedApplication to ensure app is active
            var nsAppClass = ObjCInterop.GetClass("NSApplication");
            var sharedAppSel = ObjCInterop.RegisterSelector("sharedApplication");
            var nsApp = ObjCInterop.SendMessage(nsAppClass, sharedAppSel);
            Console.WriteLine($"[StatusBar] NSApplication.sharedApplication: {nsApp}");

            // Check if app is running
            var isRunningSel = ObjCInterop.RegisterSelector("isRunning");
            var isRunning = ObjCInterop.SendMessageBool(nsApp, isRunningSel);
            Console.WriteLine($"[StatusBar] App isRunning: {isRunning}");

            // Get NSStatusBar.systemStatusBar
            var nsStatusBarClass = ObjCInterop.GetClass("NSStatusBar");
            Console.WriteLine($"[StatusBar] NSStatusBar class: {nsStatusBarClass}");
            if (nsStatusBarClass == IntPtr.Zero) { Console.WriteLine("[StatusBar] Failed to get NSStatusBar class"); return; }

            var systemStatusBar = ObjCInterop.SendMessage(nsStatusBarClass, SystemStatusBarSel);
            Console.WriteLine($"[StatusBar] systemStatusBar: {systemStatusBar}");
            if (systemStatusBar == IntPtr.Zero) { Console.WriteLine("[StatusBar] Failed to get systemStatusBar"); return; }

            // Create status item with variable length
            Console.WriteLine("[StatusBar] Creating status item...");
            _statusItem = ObjCInterop.SendMessage(systemStatusBar, StatusItemWithLengthSel, NSVariableStatusItemLength);
            Console.WriteLine($"[StatusBar] statusItem: {_statusItem}");
            if (_statusItem == IntPtr.Zero) { Console.WriteLine("[StatusBar] Failed to create status item"); return; }

            // Retain the status item to prevent it from being released
            Console.WriteLine("[StatusBar] Retaining status item...");
            ObjCInterop.SendMessage(_statusItem, ObjCInterop.RetainSelector);
            Console.WriteLine("[StatusBar] Status item retained");

            // Explicitly set visible
            Console.WriteLine("[StatusBar] Setting visible...");
            ObjCInterop.SendMessageVoid(_statusItem, SetVisibleSel, true);

            // Get the button and set title
            Console.WriteLine("[StatusBar] Getting button...");
            var button = ObjCInterop.SendMessage(_statusItem, ButtonSel);
            Console.WriteLine($"[StatusBar] button: {button}");
            if (button != IntPtr.Zero)
            {
                // Set a clearly visible title (calendar emoji + day number)
                Console.WriteLine("[StatusBar] Setting button title...");
                var title = ObjCInterop.CreateNSString($"📅 {DateTime.Now.Day}");
                Console.WriteLine($"[StatusBar] title NSString: {title}");
                ObjCInterop.SendMessageVoid(button, SetTitleSel, title);
                Console.WriteLine("[StatusBar] Title set on button");
                ObjCInterop.Release(title);

                // Set tooltip
                Console.WriteLine("[StatusBar] Setting tooltip...");
                var tooltip = ObjCInterop.CreateNSString("Sundy Calendar");
                ObjCInterop.SendMessageVoid(button, SetToolTipSel, tooltip);
                ObjCInterop.Release(tooltip);
            }
            else
            {
                Console.WriteLine("[StatusBar] WARNING: button is null!");
            }

            // Create and set menu
            Console.WriteLine("[StatusBar] Creating menu...");
            CreateMenu();
            Console.WriteLine("[StatusBar] Initialization complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StatusBar] Exception: {ex}");
            System.Diagnostics.Debug.WriteLine($"Failed to initialize macOS status bar: {ex.Message}");
        }
    }

    private void CreateMenu()
    {
        Console.WriteLine("[StatusBar] CreateMenu starting...");

        // Create NSMenu
        _menu = ObjCInterop.CreateInstance("NSMenu");
        Console.WriteLine($"[StatusBar] menu: {_menu}");
        if (_menu == IntPtr.Zero) { Console.WriteLine("[StatusBar] Failed to create menu"); return; }

        // Initialize displayed month to current
        _displayedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // Add month minimap calendar view at the top
        AddCalendarMinimapToMenu();

        // Add separator after calendar
        Console.WriteLine("[StatusBar] Adding separator after calendar...");
        var nsMenuItemClass = ObjCInterop.GetClass("NSMenuItem");
        var separator = ObjCInterop.SendMessage(nsMenuItemClass, SeparatorItemSel);
        ObjCInterop.SendMessageVoid(_menu, AddItemSel, separator);

        // Add "Open Sundy" item
        Console.WriteLine("[StatusBar] Creating openTitle NSString...");
        var openTitle = ObjCInterop.CreateNSString("Open Sundy");
        var emptyKey = ObjCInterop.CreateNSString("");

        Console.WriteLine("[StatusBar] Adding Open Sundy menu item...");
        ObjCInterop.SendMessage(_menu, AddItemWithTitleActionKeyEquivalentSel, openTitle, IntPtr.Zero, emptyKey);
        Console.WriteLine("[StatusBar] Open Sundy item added");
        ObjCInterop.Release(openTitle);

        // Add separator
        Console.WriteLine("[StatusBar] Adding separator...");
        var separator2 = ObjCInterop.SendMessage(nsMenuItemClass, SeparatorItemSel);
        ObjCInterop.SendMessageVoid(_menu, AddItemSel, separator2);
        Console.WriteLine("[StatusBar] Separator added");

        // Add "Nothing scheduled" placeholder
        Console.WriteLine("[StatusBar] Creating nothingTitle NSString...");
        var nothingTitle = ObjCInterop.CreateNSString("Nothing scheduled");

        Console.WriteLine("[StatusBar] Adding Nothing scheduled menu item...");
        ObjCInterop.SendMessage(_menu, AddItemWithTitleActionKeyEquivalentSel, nothingTitle, IntPtr.Zero, emptyKey);
        Console.WriteLine("[StatusBar] Nothing scheduled item added");
        ObjCInterop.Release(nothingTitle);
        ObjCInterop.Release(emptyKey);

        // Set menu on status item
        Console.WriteLine("[StatusBar] Setting menu on status item...");
        ObjCInterop.SendMessageVoid(_statusItem, SetMenuSel, _menu);
        Console.WriteLine("[StatusBar] CreateMenu complete");
    }

    private void AddCalendarMinimapToMenu()
    {
        Console.WriteLine("[StatusBar] Adding calendar minimap...");

        try
        {
            // Create the calendar view
            _calendarView = new MenuBarCalendarView(
                onDateClicked: date =>
                {
                    Console.WriteLine($"[StatusBar] Date clicked: {date}");
                    DateClicked?.Invoke(date);
                },
                onMonthChanged: delta =>
                {
                    _displayedMonth = _displayedMonth.AddMonths(delta);
                    _calendarView?.SetDisplayedMonth(_displayedMonth);
                }
            );
            _calendarView.Initialize();

            if (_calendarView.View == IntPtr.Zero)
            {
                Console.WriteLine("[StatusBar] Failed to create calendar view");
                return;
            }

            // Create an NSMenuItem to hold the custom view
            var nsMenuItemClass = ObjCInterop.GetClass("NSMenuItem");
            var allocSel = ObjCInterop.AllocSelector;
            var initSel = ObjCInterop.InitSelector;

            var menuItem = ObjCInterop.SendMessage(nsMenuItemClass, allocSel);
            menuItem = ObjCInterop.SendMessage(menuItem, initSel);

            if (menuItem == IntPtr.Zero)
            {
                Console.WriteLine("[StatusBar] Failed to create menu item for calendar");
                return;
            }

            // Set the custom view on the menu item
            ObjCInterop.SendMessageVoid(menuItem, SetViewSel, _calendarView.View);

            // Add the menu item to the menu
            ObjCInterop.SendMessageVoid(_menu, AddItemSel, menuItem);

            Console.WriteLine("[StatusBar] Calendar minimap added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StatusBar] Error adding calendar minimap: {ex}");
        }
    }

    public void UpdateDayNumber(int day)
    {
        if (_statusItem == IntPtr.Zero) return;

        var button = ObjCInterop.SendMessage(_statusItem, ButtonSel);
        if (button == IntPtr.Zero) return;

        var title = ObjCInterop.CreateNSString(day.ToString());
        ObjCInterop.SendMessageVoid(button, SetTitleSel, title);
        ObjCInterop.Release(title);
    }

    public void UpdateNextEvent(string? eventTitle, DateTime? eventTime)
    {
        // TODO: Update menu item with next event info
        // This requires more complex menu manipulation
    }

    public void UpdateDisplayedMonth(int year, int month)
    {
        _displayedMonth = new DateTime(year, month, 1);
        _calendarView?.SetDisplayedMonth(_displayedMonth);
    }

    public void Shutdown()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            // Dispose calendar view first
            _calendarView?.Dispose();
            _calendarView = null;

            if (_statusItem != IntPtr.Zero)
            {
                // Remove from status bar
                var nsStatusBarClass = ObjCInterop.GetClass("NSStatusBar");
                var systemStatusBar = ObjCInterop.SendMessage(nsStatusBarClass, SystemStatusBarSel);
                if (systemStatusBar != IntPtr.Zero)
                {
                    ObjCInterop.SendMessageVoid(systemStatusBar, RemoveStatusItemSel, _statusItem);
                }
                _statusItem = IntPtr.Zero;
            }

            if (_menu != IntPtr.Zero)
            {
                ObjCInterop.Release(_menu);
                _menu = IntPtr.Zero;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cleaning up status bar: {ex.Message}");
        }

        GC.SuppressFinalize(this);
    }

    ~StatusBarController()
    {
        Dispose();
    }
}
#endif
