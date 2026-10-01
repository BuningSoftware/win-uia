using System.ComponentModel;
using System.Runtime.InteropServices;
using WinUia.Input.Interop;
using static WinUia.Input.Interop.NativeMethods;

namespace WinUia.Input;

/// <summary>
/// Physical mouse and keyboard input through <c>SendInput</c>. Coordinates are physical screen pixels.
/// Physical input needs an interactive desktop, goes to whatever window is under the cursor or focused, and
/// cannot reach an elevated application unless the calling process is elevated too (UIPI).
/// </summary>
public static class Win32InputSimulator
{
    /// <summary>Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public static void MoveTo(int x, int y) => PhysicalDpi.Run(() =>
        Send([MouseInput(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK, x, y)]));

    /// <summary>Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) and clicks.</summary>
    public static void ClickAt(int x, int y, MouseButton button = MouseButton.Left)
    {
        MoveTo(x, y);
        Thread.Sleep(20);

        var (down, up) = button == MouseButton.Left
            ? (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP)
            : (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP);
        PhysicalDpi.Run(() => Send([MouseInput(down, 0, 0, absolute: false), MouseInput(up, 0, 0, absolute: false)]));
    }

    /// <summary>Types <paramref name="text"/> into the focused control as Unicode characters.</summary>
    public static void SendText(string text)
    {
        var inputs = new List<INPUT>(text.Length * 2);
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                continue; // "\r\n" is one Enter.

            if (ch is '\r' or '\n')
            {
                inputs.Add(KeyInput((ushort)VirtualKey.Enter, 0, 0));
                inputs.Add(KeyInput((ushort)VirtualKey.Enter, 0, KEYEVENTF_KEYUP));
                continue;
            }

            inputs.Add(KeyInput(0, ch, KEYEVENTF_UNICODE));
            inputs.Add(KeyInput(0, ch, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP));
        }

        if (inputs.Count > 0)
            Send([.. inputs]);
    }

    /// <summary>
    /// Presses the keys in order and releases them in reverse order: one key (<c>SendKeys(VirtualKey.Enter)</c>) or
    /// a combination (<c>SendKeys(VirtualKey.Control, VirtualKey.A)</c>).
    /// </summary>
    public static void SendKeys(params VirtualKey[] keys)
    {
        var inputs = new List<INPUT>(keys.Length * 2);
        inputs.AddRange(keys.Select(k => KeyInput((ushort)k, 0, 0)));
        inputs.AddRange(keys.Reverse().Select(k => KeyInput((ushort)k, 0, KEYEVENTF_KEYUP)));
        Send([.. inputs]);
    }

    private static INPUT MouseInput(uint flags, int x, int y, bool absolute = true)
    {
        var mi = new MOUSEINPUT { dwFlags = flags };
        if (absolute)
        {
            // Absolute coordinates are normalised to 0..65535 across the virtual desktop.
            var left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            var top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            var width = Math.Max(GetSystemMetrics(SM_CXVIRTUALSCREEN), 2);
            var height = Math.Max(GetSystemMetrics(SM_CYVIRTUALSCREEN), 2);
            mi.dx = (int)Math.Round((x - left) * 65535.0 / (width - 1));
            mi.dy = (int)Math.Round((y - top) * 65535.0 / (height - 1));
        }

        return new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = mi } };
    }

    private static INPUT KeyInput(ushort vk, ushort scan, uint flags) =>
        new() { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags } } };

    private static void Send(INPUT[] inputs)
    {
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SendInput was blocked; is the desktop locked or the target elevated?");
    }
}
