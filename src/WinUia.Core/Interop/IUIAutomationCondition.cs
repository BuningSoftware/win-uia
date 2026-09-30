using System.Runtime.InteropServices;

// Declared in UIAutomationClient.h vtable order; see IUIAutomation.cs for the interop rules.
namespace WinUia.Core.Interop;

[ComImport, Guid("352ffba8-0973-437c-a61f-f64cafd81df9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationCondition;
