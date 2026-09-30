using WinUia.Input.Interop;

namespace WinUia.Input;

/// <summary>
/// The physical-pixel coordinate space shared by UIA reads and <c>SendInput</c>. Code that reads screen coordinates
/// from UIA runs inside <see cref="Run{T}"/>, the same scope <see cref="Win32InputSimulator"/> sends input in.
/// </summary>
public static class PhysicalDpi
{
    /// <summary>
    /// Makes the process per-monitor-v2 DPI aware, for UIA builds that only honour process awareness. Fails
    /// harmlessly when the awareness was already set (for example by an application manifest).
    /// </summary>
    public static void MakeProcessAware() =>
        _ = NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.DpiAwarenessContextPerMonitorAwareV2);

    /// <summary>Runs <paramref name="action"/> with the calling thread temporarily per-monitor-v2 DPI aware.</summary>
    public static T Run<T>(Func<T> action)
    {
        var previous = NativeMethods.SetThreadDpiAwarenessContext(NativeMethods.DpiAwarenessContextPerMonitorAwareV2);
        try
        {
            return action();
        }
        finally
        {
            if (previous != 0)
                NativeMethods.SetThreadDpiAwarenessContext(previous);
        }
    }

    /// <inheritdoc cref="Run{T}"/>
    public static void Run(Action action) => Run(() => { action(); return true; });
}
