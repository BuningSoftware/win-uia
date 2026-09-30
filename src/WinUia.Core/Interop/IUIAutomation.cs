using System.Runtime.InteropServices;

// Hand-written COM interop for the UI Automation client API (UIAutomationClient.h, Windows SDK).
//
// The vtable order is the contract: every interface declares its methods in exactly the order of the
// C vtable in UIAutomationClient.h (IUnknown's three slots are implied by InterfaceIsIUnknown). Methods
// WinUia does not use are declared as "Reserved_<RealName>" stubs so the following slots stay aligned;
// they must never be called. Slots after the last method used are simply not declared.
//
// UIA uses Win32 BOOL (4 bytes), not VARIANT_BOOL, so every bool is marshalled as UnmanagedType.Bool.
namespace WinUia.Core.Interop;

[ComImport, Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomation
{
    [return: MarshalAs(UnmanagedType.Bool)]
    bool CompareElements(IUIAutomationElement el1, IUIAutomationElement el2);
    void Reserved_CompareRuntimeIds();
    IUIAutomationElement GetRootElement();
    IUIAutomationElement ElementFromHandle(nint hwnd);
    IUIAutomationElement ElementFromPoint(tagPOINT pt);
    IUIAutomationElement GetFocusedElement();
    void Reserved_GetRootElementBuildCache();
    void Reserved_ElementFromHandleBuildCache();
    void Reserved_ElementFromPointBuildCache();
    void Reserved_GetFocusedElementBuildCache();
    void Reserved_CreateTreeWalker();
    IUIAutomationTreeWalker ControlViewWalker { get; }
    void Reserved_get_ContentViewWalker();
    void Reserved_get_RawViewWalker();
    void Reserved_get_RawViewCondition();
    void Reserved_get_ControlViewCondition();
    void Reserved_get_ContentViewCondition();
    void Reserved_CreateCacheRequest();
    IUIAutomationCondition CreateTrueCondition();
    void Reserved_CreateFalseCondition();
    IUIAutomationCondition CreatePropertyCondition(int propertyId, [MarshalAs(UnmanagedType.Struct)] object value);
    void Reserved_CreatePropertyConditionEx();
    IUIAutomationCondition CreateAndCondition(IUIAutomationCondition condition1, IUIAutomationCondition condition2);
    void Reserved_CreateAndConditionFromArray();
    void Reserved_CreateAndConditionFromNativeArray();
    IUIAutomationCondition CreateOrCondition(IUIAutomationCondition condition1, IUIAutomationCondition condition2);
    void Reserved_CreateOrConditionFromArray();
    void Reserved_CreateOrConditionFromNativeArray();
    IUIAutomationCondition CreateNotCondition(IUIAutomationCondition condition);
}
