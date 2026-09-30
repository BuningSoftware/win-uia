using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct tagPOINT
{
    public int x;
    public int y;
}
