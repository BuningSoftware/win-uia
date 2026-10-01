using System.Runtime.InteropServices;

// Declared in UIAutomationClient.h vtable order; see IUIAutomation.cs for the interop rules.
namespace WinUia.Core.Interop;

[ComImport, Guid("fb377fbe-8ea6-46d5-9c73-6499642d3059"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationInvokePattern
{
    void Invoke();
}
