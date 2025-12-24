#if MACOS_DESKTOP
using System;
using System.Runtime.InteropServices;

namespace Sundy.Uno.Platforms.Desktop.MacOS;

/// <summary>
/// Creates a native NSView-based calendar minimap for the menu bar.
/// Shows a compact month grid with clickable days.
/// </summary>
public class MenuBarCalendarView : IDisposable
{
    private IntPtr _containerView;
    private IntPtr _monthLabel;
    private DateTime _displayedMonth;
    private bool _disposed;
    private readonly Action<DateTime>? _onDateClicked;
    private readonly Action<int>? _onMonthChanged; // -1 for prev, +1 for next

    // View dimensions
    private const double ViewWidth = 220;
    private const double DayCellSize = 28;
    private const double HeaderHeight = 32;
    private const double WeekdayHeaderHeight = 20;
    private const double Padding = 8;

    // Cached selectors
    private static readonly IntPtr SetFrameSel = ObjCInterop.RegisterSelector("setFrame:");
    private static readonly IntPtr AddSubviewSel = ObjCInterop.RegisterSelector("addSubview:");
    private static readonly IntPtr SetStringValueSel = ObjCInterop.RegisterSelector("setStringValue:");
    private static readonly IntPtr SetAlignmentSel = ObjCInterop.RegisterSelector("setAlignment:");
    private static readonly IntPtr SetFontSel = ObjCInterop.RegisterSelector("setFont:");
    private static readonly IntPtr SetTextColorSel = ObjCInterop.RegisterSelector("setTextColor:");
    private static readonly IntPtr SetBackgroundColorSel = ObjCInterop.RegisterSelector("setBackgroundColor:");
    private static readonly IntPtr SetDrawsBackgroundSel = ObjCInterop.RegisterSelector("setDrawsBackground:");
    private static readonly IntPtr SetBorderedSel = ObjCInterop.RegisterSelector("setBordered:");
    private static readonly IntPtr SetBezelStyleSel = ObjCInterop.RegisterSelector("setBezelStyle:");
    private static readonly IntPtr SetWantsLayerSel = ObjCInterop.RegisterSelector("setWantsLayer:");
    private static readonly IntPtr LayerSel = ObjCInterop.RegisterSelector("layer");
    private static readonly IntPtr SetCornerRadiusSel = ObjCInterop.RegisterSelector("setCornerRadius:");
    private static readonly IntPtr SystemFontOfSizeSel = ObjCInterop.RegisterSelector("systemFontOfSize:");
    private static readonly IntPtr BoldSystemFontOfSizeSel = ObjCInterop.RegisterSelector("boldSystemFontOfSize:");
    private static readonly IntPtr SetEditableSel = ObjCInterop.RegisterSelector("setEditable:");
    private static readonly IntPtr SetSelectableSel = ObjCInterop.RegisterSelector("setSelectable:");
    private static readonly IntPtr ColorWithSRGBRedSel = ObjCInterop.RegisterSelector("colorWithSRGBRed:green:blue:alpha:");
    private static readonly IntPtr WhiteColorSel = ObjCInterop.RegisterSelector("whiteColor");
    private static readonly IntPtr ClearColorSel = ObjCInterop.RegisterSelector("clearColor");
    private static readonly IntPtr SetTitleSel = ObjCInterop.RegisterSelector("setTitle:");
    private static readonly IntPtr SetTargetSel = ObjCInterop.RegisterSelector("setTarget:");
    private static readonly IntPtr SetActionSel = ObjCInterop.RegisterSelector("setAction:");
    private static readonly IntPtr SetTagSel = ObjCInterop.RegisterSelector("setTag:");
    private static readonly IntPtr TagSel = ObjCInterop.RegisterSelector("tag");

    public MenuBarCalendarView(Action<DateTime>? onDateClicked = null, Action<int>? onMonthChanged = null)
    {
        _onDateClicked = onDateClicked;
        _onMonthChanged = onMonthChanged;
        _displayedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    }

    public IntPtr View => _containerView;

    public double ViewHeight => HeaderHeight + WeekdayHeaderHeight + (6 * DayCellSize) + (Padding * 2) + 8;

    public void Initialize()
    {
        CreateContainerView();
        BuildCalendarView();
    }

    public void SetDisplayedMonth(DateTime month)
    {
        _displayedMonth = new DateTime(month.Year, month.Month, 1);
        UpdateCalendarView();
    }

    private void CreateContainerView()
    {
        // Create the container NSView
        _containerView = ObjCInterop.CreateInstance("NSView");
        if (_containerView == IntPtr.Zero) return;

        // Set frame
        SetViewFrame(_containerView, 0, 0, ViewWidth, ViewHeight);

        // Enable layer-backed view for better rendering
        ObjCInterop.SendMessageVoid(_containerView, SetWantsLayerSel, true);
    }

    private void BuildCalendarView()
    {
        if (_containerView == IntPtr.Zero) return;

        double yOffset = ViewHeight - Padding;

        // Month header with navigation
        yOffset -= HeaderHeight;
        CreateMonthHeader(yOffset);

        // Weekday labels
        yOffset -= WeekdayHeaderHeight;
        CreateWeekdayLabels(yOffset);

        // Day grid
        yOffset -= 4; // Small gap
        CreateDayGrid(yOffset);
    }

    private void CreateMonthHeader(double yOffset)
    {
        var nsColorClass = ObjCInterop.GetClass("NSColor");
        var nsFontClass = ObjCInterop.GetClass("NSFont");

        // Previous month button
        var prevButton = CreateButton("<", Padding, yOffset, 28, 28);
        if (prevButton != IntPtr.Zero)
        {
            SetButtonTag(prevButton, -1);
            ObjCInterop.SendMessageVoid(_containerView, AddSubviewSel, prevButton);
        }

        // Month/Year label
        _monthLabel = ObjCInterop.CreateInstance("NSTextField");
        if (_monthLabel != IntPtr.Zero)
        {
            SetViewFrame(_monthLabel, Padding + 32, yOffset, ViewWidth - 72, HeaderHeight);

            var monthText = ObjCInterop.CreateNSString(_displayedMonth.ToString("MMMM yyyy"));
            ObjCInterop.SendMessageVoid(_monthLabel, SetStringValueSel, monthText);
            ObjCInterop.Release(monthText);

            // Center alignment (2 = NSTextAlignmentCenter)
            ObjCInterop.SendMessageVoid(_monthLabel, SetAlignmentSel, (IntPtr)1);

            // Font
            var font = ObjCInterop.SendMessage(nsFontClass, BoldSystemFontOfSizeSel, 13.0);
            ObjCInterop.SendMessageVoid(_monthLabel, SetFontSel, font);

            // Text color (light gray for dark theme)
            var textColor = CreateColor(0.9, 0.9, 0.9, 1.0);
            ObjCInterop.SendMessageVoid(_monthLabel, SetTextColorSel, textColor);

            // No background, not editable
            ObjCInterop.SendMessageVoid(_monthLabel, SetDrawsBackgroundSel, false);
            ObjCInterop.SendMessageVoid(_monthLabel, SetEditableSel, false);
            ObjCInterop.SendMessageVoid(_monthLabel, SetSelectableSel, false);
            ObjCInterop.SendMessageVoid(_monthLabel, SetBorderedSel, false);

            ObjCInterop.SendMessageVoid(_containerView, AddSubviewSel, _monthLabel);
        }

        // Next month button
        var nextButton = CreateButton(">", ViewWidth - Padding - 28, yOffset, 28, 28);
        if (nextButton != IntPtr.Zero)
        {
            SetButtonTag(nextButton, 1);
            ObjCInterop.SendMessageVoid(_containerView, AddSubviewSel, nextButton);
        }
    }

    private void CreateWeekdayLabels(double yOffset)
    {
        var nsFontClass = ObjCInterop.GetClass("NSFont");
        string[] weekdays = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

        double cellWidth = (ViewWidth - (Padding * 2)) / 7;

        for (int i = 0; i < 7; i++)
        {
            var label = ObjCInterop.CreateInstance("NSTextField");
            if (label == IntPtr.Zero) continue;

            double x = Padding + (i * cellWidth);
            SetViewFrame(label, x, yOffset, cellWidth, WeekdayHeaderHeight);

            var text = ObjCInterop.CreateNSString(weekdays[i]);
            ObjCInterop.SendMessageVoid(label, SetStringValueSel, text);
            ObjCInterop.Release(text);

            // Center alignment
            ObjCInterop.SendMessageVoid(label, SetAlignmentSel, (IntPtr)1);

            // Font
            var font = ObjCInterop.SendMessage(nsFontClass, SystemFontOfSizeSel, 10.0);
            ObjCInterop.SendMessageVoid(label, SetFontSel, font);

            // Muted text color
            var textColor = CreateColor(0.6, 0.6, 0.6, 1.0);
            ObjCInterop.SendMessageVoid(label, SetTextColorSel, textColor);

            ObjCInterop.SendMessageVoid(label, SetDrawsBackgroundSel, false);
            ObjCInterop.SendMessageVoid(label, SetEditableSel, false);
            ObjCInterop.SendMessageVoid(label, SetSelectableSel, false);
            ObjCInterop.SendMessageVoid(label, SetBorderedSel, false);

            ObjCInterop.SendMessageVoid(_containerView, AddSubviewSel, label);
        }
    }

    private void CreateDayGrid(double startY)
    {
        var nsFontClass = ObjCInterop.GetClass("NSFont");
        double cellWidth = (ViewWidth - (Padding * 2)) / 7;

        var firstDay = _displayedMonth;
        int startDayOfWeek = (int)firstDay.DayOfWeek;
        int daysInMonth = DateTime.DaysInMonth(_displayedMonth.Year, _displayedMonth.Month);
        var today = DateTime.Today;

        int row = 0;
        int col = startDayOfWeek;

        for (int day = 1; day <= daysInMonth; day++)
        {
            double x = Padding + (col * cellWidth);
            double y = startY - (row * DayCellSize) - DayCellSize;

            var currentDate = new DateTime(_displayedMonth.Year, _displayedMonth.Month, day);
            bool isToday = currentDate.Date == today;

            // Create button for each day
            var dayButton = CreateDayButton(day.ToString(), x + 2, y, cellWidth - 4, DayCellSize - 2, isToday);
            if (dayButton != IntPtr.Zero)
            {
                // Store the day as tag for click handling
                SetButtonTag(dayButton, day);
                ObjCInterop.SendMessageVoid(_containerView, AddSubviewSel, dayButton);
            }

            col++;
            if (col > 6)
            {
                col = 0;
                row++;
            }
        }
    }

    private IntPtr CreateButton(string title, double x, double y, double width, double height)
    {
        var button = ObjCInterop.CreateInstance("NSButton");
        if (button == IntPtr.Zero) return IntPtr.Zero;

        SetViewFrame(button, x, y, width, height);

        var titleStr = ObjCInterop.CreateNSString(title);
        ObjCInterop.SendMessageVoid(button, SetTitleSel, titleStr);
        ObjCInterop.Release(titleStr);

        // Borderless style
        ObjCInterop.SendMessageVoid(button, SetBorderedSel, false);

        // Enable layer for rounded corners
        ObjCInterop.SendMessageVoid(button, SetWantsLayerSel, true);

        return button;
    }

    private IntPtr CreateDayButton(string title, double x, double y, double width, double height, bool isToday)
    {
        var button = ObjCInterop.CreateInstance("NSButton");
        if (button == IntPtr.Zero) return IntPtr.Zero;

        SetViewFrame(button, x, y, width, height);

        var titleStr = ObjCInterop.CreateNSString(title);
        ObjCInterop.SendMessageVoid(button, SetTitleSel, titleStr);
        ObjCInterop.Release(titleStr);

        // Borderless style
        ObjCInterop.SendMessageVoid(button, SetBorderedSel, false);

        // Enable layer for styling
        ObjCInterop.SendMessageVoid(button, SetWantsLayerSel, true);
        var layer = ObjCInterop.SendMessage(button, LayerSel);
        if (layer != IntPtr.Zero)
        {
            // Set corner radius
            SetLayerCornerRadius(layer, height / 2);

            if (isToday)
            {
                // Accent background for today
                SetLayerBackgroundColor(layer, 0.31, 0.16, 0.57, 1.0); // AccentPrimary #4F2991
            }
        }

        return button;
    }

    private void SetButtonTag(IntPtr button, int tag)
    {
        // Use nint for tag to handle platform differences
        ObjCInterop.SendMessageVoid(button, SetTagSel, (IntPtr)tag);
    }

    private void UpdateCalendarView()
    {
        if (_monthLabel == IntPtr.Zero) return;

        // Update month label text
        var monthText = ObjCInterop.CreateNSString(_displayedMonth.ToString("MMMM yyyy"));
        ObjCInterop.SendMessageVoid(_monthLabel, SetStringValueSel, monthText);
        ObjCInterop.Release(monthText);

        // For a full update, we'd need to remove and recreate day buttons
        // This is a simplified version - full implementation would track and update day buttons
    }

    private IntPtr CreateColor(double r, double g, double b, double a)
    {
        var nsColorClass = ObjCInterop.GetClass("NSColor");
        return ObjCInterop.SendMessage(nsColorClass, ColorWithSRGBRedSel, r, g, b, a);
    }

    private void SetViewFrame(IntPtr view, double x, double y, double width, double height)
    {
        // NSRect is a struct, we need to use a different approach
        // For simplicity, we'll use initWithFrame: during creation
        var setFrameOriginSel = ObjCInterop.RegisterSelector("setFrameOrigin:");
        var setFrameSizeSel = ObjCInterop.RegisterSelector("setFrameSize:");

        // Create NSPoint for origin
        SetFrameOriginAndSize(view, x, y, width, height);
    }

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageNSRect(IntPtr receiver, IntPtr selector, NSRect rect);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageNSPoint(IntPtr receiver, IntPtr selector, NSPoint point);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageNSSize(IntPtr receiver, IntPtr selector, NSSize size);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageCGFloat(IntPtr receiver, IntPtr selector, double value);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageFourDoubles(IntPtr receiver, IntPtr selector, double r, double g, double b, double a);

    private void SetFrameOriginAndSize(IntPtr view, double x, double y, double width, double height)
    {
        var rect = new NSRect { X = x, Y = y, Width = width, Height = height };
        SendMessageNSRect(view, SetFrameSel, rect);
    }

    private void SetLayerCornerRadius(IntPtr layer, double radius)
    {
        SendMessageCGFloat(layer, SetCornerRadiusSel, radius);
    }

    private void SetLayerBackgroundColor(IntPtr layer, double r, double g, double b, double a)
    {
        var setBackgroundColorSel = ObjCInterop.RegisterSelector("setBackgroundColor:");
        var cgColorClass = ObjCInterop.GetClass("NSColor");
        var color = ObjCInterop.SendMessage(cgColorClass, ColorWithSRGBRedSel, r, g, b, a);

        // Get CGColor from NSColor
        var cgColorSel = ObjCInterop.RegisterSelector("CGColor");
        var cgColor = ObjCInterop.SendMessage(color, cgColorSel);

        ObjCInterop.SendMessageVoid(layer, setBackgroundColorSel, cgColor);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSSize
    {
        public double Width;
        public double Height;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_containerView != IntPtr.Zero)
        {
            ObjCInterop.Release(_containerView);
            _containerView = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }

    ~MenuBarCalendarView()
    {
        Dispose();
    }
}
#endif
