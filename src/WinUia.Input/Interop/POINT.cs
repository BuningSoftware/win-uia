using System.Runtime.InteropServices;

namespace WinUia.Input.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int x;
    public int y;
}
