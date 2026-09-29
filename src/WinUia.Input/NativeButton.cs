using WinUia.Input.Interop;

namespace WinUia.Input;

/// <summary>Clicks native Win32 push buttons ("BUTTON" window class) through their window's message queue.</summary>
internal static class NativeButton
{
    /// <summary>
    /// Posts <c>BM_CLICK</c> to the button, so the click runs from the application's own message loop rather
    /// than inside the caller's call. Returns false when the message could not be posted.
    /// </summary>
    public static bool PostClick(nint hwnd) => NativeMethods.PostMessage(hwnd, NativeMethods.BM_CLICK, 0, 0);
}
