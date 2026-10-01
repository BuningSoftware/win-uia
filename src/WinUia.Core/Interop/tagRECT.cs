using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct tagRECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
