using System.Runtime.InteropServices;

// Declared in UIAutomationClient.h vtable order; see IUIAutomation.cs for the interop rules.
namespace WinUia.Core.Interop;

[ComImport, Guid("32eba289-3583-42c9-9c59-3b6d9a1e9b6a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTextPattern
{
    void Reserved_RangeFromPoint();
    void Reserved_RangeFromChild();
    void Reserved_GetSelection();
    void Reserved_GetVisibleRanges();
    IUIAutomationTextRange DocumentRange { get; }
}
