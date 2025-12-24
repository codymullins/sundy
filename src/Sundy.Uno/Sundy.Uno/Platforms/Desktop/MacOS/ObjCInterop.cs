#if MACOS_DESKTOP
using System;
using System.Runtime.InteropServices;

namespace Sundy.Uno.Platforms.Desktop.MacOS;

/// <summary>
/// Low-level Objective-C runtime interop for macOS native features.
/// Uses P/Invoke to call Objective-C methods directly without framework references.
/// </summary>
internal static class ObjCInterop
{
    private const string ObjCLib = "/usr/lib/libobjc.A.dylib";
    private const string AppKitLib = "/System/Library/Frameworks/AppKit.framework/AppKit";
    private const string LibDispatch = "/usr/lib/libSystem.B.dylib";

    // libdispatch for main queue operations
    [DllImport(LibDispatch)]
    public static extern IntPtr dispatch_get_main_queue();

    [DllImport(LibDispatch)]
    public static extern void dispatch_async_f(IntPtr queue, IntPtr context, IntPtr work);

    // Delegate for dispatch work
    public delegate void DispatchFunction(IntPtr context);

    // dlopen for loading frameworks
    [DllImport(LibDispatch)]
    public static extern IntPtr dlopen(string path, int mode);

    private const int RTLD_NOW = 2;

    private static bool _appKitLoaded;

    /// <summary>
    /// Ensures AppKit framework is loaded.
    /// </summary>
    public static void EnsureAppKitLoaded()
    {
        if (_appKitLoaded) return;

        Console.WriteLine("[ObjCInterop] Loading AppKit framework...");
        var handle = dlopen("/System/Library/Frameworks/AppKit.framework/AppKit", RTLD_NOW);
        Console.WriteLine($"[ObjCInterop] AppKit handle: {handle}");
        _appKitLoaded = handle != IntPtr.Zero;

        if (!_appKitLoaded)
        {
            Console.WriteLine("[ObjCInterop] WARNING: Failed to load AppKit!");
        }
    }

    [DllImport(ObjCLib, EntryPoint = "objc_getClass")]
    public static extern IntPtr GetClass(string name);

    [DllImport(ObjCLib, EntryPoint = "sel_registerName")]
    public static extern IntPtr RegisterSelector(string name);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, double arg1);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, double arg1, double arg2, double arg3, double arg4);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
    public static extern bool SendMessageBool(IntPtr receiver, IntPtr selector);

    // Common selectors cached for performance
    private static IntPtr? _allocSel;
    private static IntPtr? _initSel;
    private static IntPtr? _releaseSel;
    private static IntPtr? _retainSel;

    public static IntPtr AllocSelector => _allocSel ??= RegisterSelector("alloc");
    public static IntPtr InitSelector => _initSel ??= RegisterSelector("init");
    public static IntPtr ReleaseSelector => _releaseSel ??= RegisterSelector("release");
    public static IntPtr RetainSelector => _retainSel ??= RegisterSelector("retain");

    /// <summary>
    /// Creates a new instance of an Objective-C class.
    /// </summary>
    public static IntPtr CreateInstance(string className)
    {
        var cls = GetClass(className);
        if (cls == IntPtr.Zero) return IntPtr.Zero;

        var instance = SendMessage(cls, AllocSelector);
        instance = SendMessage(instance, InitSelector);
        return instance;
    }

    /// <summary>
    /// Creates an NSString from a C# string.
    /// </summary>
    public static IntPtr CreateNSString(string? str)
    {
        str ??= "";

        var nsStringClass = GetClass("NSString");
        var alloced = SendMessage(nsStringClass, AllocSelector);
        var initWithUTF8StringSel = RegisterSelector("initWithUTF8String:");

        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
        var handle = GCHandle.Alloc(utf8Bytes, GCHandleType.Pinned);
        try
        {
            return SendMessage(alloced, initWithUTF8StringSel, handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Releases an Objective-C object.
    /// </summary>
    public static void Release(IntPtr obj)
    {
        if (obj != IntPtr.Zero)
        {
            SendMessageVoid(obj, ReleaseSelector);
        }
    }
}
#endif
