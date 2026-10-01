using System.Runtime.InteropServices;

namespace WinUia.Input.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct INPUT
{
    public uint type;
    public InputUnion u;
}
