using System.ComponentModel;
using System.Diagnostics;
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
    // Pause between the positions of a smooth move: smooth to the eye and in recordings (in practice the timer gives ~15 ms).
    private static readonly TimeSpan MoveStepInterval = TimeSpan.FromMilliseconds(10);

    // How often MoveTo sends a move before giving up, and how long it waits each time for the cursor to arrive.
    private const int MoveAttempts = 3;
    private static readonly TimeSpan MoveArrivalTimeout = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) and makes sure it got there. Windows now and
    /// then drops an injected move (seen right after an application starts), and a click after a dropped move would
    /// land wherever the cursor happens to be; so the move is checked and sent again. Throws
    /// <see cref="InvalidOperationException"/> when the cursor still is not there, for example because the point is
    /// off every screen or someone is moving the mouse at the same time.
    /// </summary>
    public static void MoveTo(int x, int y)
    {
        for (var attempt = 1; ; attempt++)
        {
            SendMove(x, y);
            if (CursorArrives(x, y))
                return;

            if (attempt == MoveAttempts)
            {
                throw new InvalidOperationException(
                    $"The cursor could not be moved to ({x}, {y}); it is at {GetCursorPosition()}. The point may be off " +
                    "every screen, or something else is moving the mouse.");
            }
        }
    }

    /// <summary>
    /// Moves the cursor from where it is to (<paramref name="x"/>, <paramref name="y"/>) over
    /// <paramref name="duration"/>, along a straight line that starts and ends slowly, so the movement can be followed
    /// on screen. Ends exactly on the target; a zero duration, or a cursor already there, moves at once.
    /// </summary>
    public static void MoveTo(int x, int y, TimeSpan duration)
    {
        var (startX, startY) = GetCursorPosition();
        if (duration > TimeSpan.Zero && (startX, startY) != (x, y))
        {
            // Positions follow the clock rather than a step count: Thread.Sleep is coarse (about 15 ms), so counting
            // steps would stretch the move well past its duration.
            var elapsed = Stopwatch.StartNew();
            for (var t = 0.0; t < 1; t = elapsed.Elapsed / duration)
            {
                var progress = EaseInOut(t);
                // Intermediate positions only animate the move; just the final one is checked (MoveTo below).
                SendMove(startX + (int)Math.Round((x - startX) * progress), startY + (int)Math.Round((y - startY) * progress));
                Thread.Sleep(MoveStepInterval);
            }
        }

        MoveTo(x, y);
    }

    /// <summary>The cursor position in physical screen pixels.</summary>
    public static (int X, int Y) GetCursorPosition() => PhysicalDpi.Run(() =>
    {
        if (!GetCursorPos(out var point))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "GetCursorPos failed; is the desktop locked?");
        return (point.x, point.y);
    });

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

    private static void SendMove(int x, int y) => PhysicalDpi.Run(() =>
        Send([MouseInput(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK, x, y)]));

    // SendInput queues the move; the cursor follows once Windows has processed it, usually at once.
    private static bool CursorArrives(int x, int y)
    {
        var waited = Stopwatch.StartNew();
        while (GetCursorPosition() != (x, y))
        {
            if (waited.Elapsed >= MoveArrivalTimeout)
                return false;
            Thread.Sleep(5);
        }

        return true;
    }

    // Quadratic ease-in-out: slow start, fast middle, slow arrival. 0 maps to 0 and 1 to 1.
    private static double EaseInOut(double t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

    private static INPUT MouseInput(uint flags, int x, int y, bool absolute = true)
    {
        var mi = new MOUSEINPUT { dwFlags = flags };
        if (absolute)
        {
            // Absolute coordinates are normalised to 0..65535 across the virtual desktop. Windows maps them back with
            // pixel = floor(value * size / 65536), so rounding up here lands on exactly the requested pixel.
            var left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            var top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            var width = Math.Max(GetSystemMetrics(SM_CXVIRTUALSCREEN), 1);
            var height = Math.Max(GetSystemMetrics(SM_CYVIRTUALSCREEN), 1);
            mi.dx = Normalise(x - left, width);
            mi.dy = Normalise(y - top, height);
        }

        return new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = mi } };
    }

    private static int Normalise(int offset, int size) =>
        Math.Clamp((int)Math.Ceiling(offset * 65536.0 / size), 0, 65535);

    private static INPUT KeyInput(ushort vk, ushort scan, uint flags) =>
        new() { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags } } };

    private static void Send(INPUT[] inputs)
    {
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SendInput was blocked; is the desktop locked or the target elevated?");
    }
}
