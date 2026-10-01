using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

/// <summary>The user32 calls Core makes itself: posting a click to a native button (see <c>Element.Click</c>).</summary>
internal static class User32
{
    public const uint BM_CLICK = 0x00F5;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);
}
