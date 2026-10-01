using System.Runtime.InteropServices;

// Declared in UIAutomationClient.h vtable order; see IUIAutomation.cs for the interop rules.
namespace WinUia.Core.Interop;

[ComImport, Guid("4042c624-389c-4afc-a630-9df854a541fc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTreeWalker
{
    IUIAutomationElement? GetParentElement(IUIAutomationElement element);
}
