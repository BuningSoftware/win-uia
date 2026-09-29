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

[ComImport, Guid("e22ad333-b25f-460c-83d0-0581107395c9"), ClassInterface(ClassInterfaceType.None)]
internal class CUIAutomation8;

[StructLayout(LayoutKind.Sequential)]
internal struct tagPOINT
{
    public int x;
    public int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct tagRECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

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

[ComImport, Guid("d22108aa-8ac5-49a5-837b-37bbb3d7591e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationElement
{
    void SetFocus();
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    int[] GetRuntimeId();
    IUIAutomationElement? FindFirst(TreeScope scope, IUIAutomationCondition condition);
    IUIAutomationElementArray FindAll(TreeScope scope, IUIAutomationCondition condition);
    void Reserved_FindFirstBuildCache();
    void Reserved_FindAllBuildCache();
    void Reserved_BuildUpdatedCache();
    [return: MarshalAs(UnmanagedType.Struct)]
    object? GetCurrentPropertyValue(int propertyId);
    void Reserved_GetCurrentPropertyValueEx();
    void Reserved_GetCachedPropertyValue();
    void Reserved_GetCachedPropertyValueEx();
    void Reserved_GetCurrentPatternAs();
    void Reserved_GetCachedPatternAs();
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object? GetCurrentPattern(int patternId);
    void Reserved_GetCachedPattern();
    void Reserved_GetCachedParent();
    void Reserved_GetCachedChildren();
    int CurrentProcessId { get; }
    int CurrentControlType { get; }
    string CurrentLocalizedControlType { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string CurrentName { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string CurrentAcceleratorKey { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string CurrentAccessKey { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentHasKeyboardFocus { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsKeyboardFocusable { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsEnabled { [return: MarshalAs(UnmanagedType.Bool)] get; }
    string CurrentAutomationId { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string CurrentClassName { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string CurrentHelpText { [return: MarshalAs(UnmanagedType.BStr)] get; }
    int CurrentCulture { get; }
    bool CurrentIsControlElement { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsContentElement { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsPassword { [return: MarshalAs(UnmanagedType.Bool)] get; }
    nint CurrentNativeWindowHandle { get; }
    string CurrentItemType { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsOffscreen { [return: MarshalAs(UnmanagedType.Bool)] get; }
    int CurrentOrientation { get; }
    string CurrentFrameworkId { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsRequiredForForm { [return: MarshalAs(UnmanagedType.Bool)] get; }
    string CurrentItemStatus { [return: MarshalAs(UnmanagedType.BStr)] get; }
    tagRECT CurrentBoundingRectangle { get; }
    void Reserved_get_CurrentLabeledBy();
    void Reserved_get_CurrentAriaRole();
    void Reserved_get_CurrentAriaProperties();
    void Reserved_get_CurrentIsDataValidForForm();
    void Reserved_get_CurrentControllerFor();
    void Reserved_get_CurrentDescribedBy();
    void Reserved_get_CurrentFlowsTo();
    void Reserved_get_CurrentProviderDescription();
    void Reserved_get_CachedProcessId();
    void Reserved_get_CachedControlType();
    void Reserved_get_CachedLocalizedControlType();
    void Reserved_get_CachedName();
    void Reserved_get_CachedAcceleratorKey();
    void Reserved_get_CachedAccessKey();
    void Reserved_get_CachedHasKeyboardFocus();
    void Reserved_get_CachedIsKeyboardFocusable();
    void Reserved_get_CachedIsEnabled();
    void Reserved_get_CachedAutomationId();
    void Reserved_get_CachedClassName();
    void Reserved_get_CachedHelpText();
    void Reserved_get_CachedCulture();
    void Reserved_get_CachedIsControlElement();
    void Reserved_get_CachedIsContentElement();
    void Reserved_get_CachedIsPassword();
    void Reserved_get_CachedNativeWindowHandle();
    void Reserved_get_CachedItemType();
    void Reserved_get_CachedIsOffscreen();
    void Reserved_get_CachedOrientation();
    void Reserved_get_CachedFrameworkId();
    void Reserved_get_CachedIsRequiredForForm();
    void Reserved_get_CachedItemStatus();
    void Reserved_get_CachedBoundingRectangle();
    void Reserved_get_CachedLabeledBy();
    void Reserved_get_CachedAriaRole();
    void Reserved_get_CachedAriaProperties();
    void Reserved_get_CachedIsDataValidForForm();
    void Reserved_get_CachedControllerFor();
    void Reserved_get_CachedDescribedBy();
    void Reserved_get_CachedFlowsTo();
    void Reserved_get_CachedProviderDescription();
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetClickablePoint(out tagPOINT clickable);
}

[ComImport, Guid("14314595-b4bc-4055-95f2-58f2e42c9855"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationElementArray
{
    int Length { get; }
    IUIAutomationElement GetElement(int index);
}

[ComImport, Guid("352ffba8-0973-437c-a61f-f64cafd81df9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationCondition;

[ComImport, Guid("4042c624-389c-4afc-a630-9df854a541fc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTreeWalker
{
    IUIAutomationElement? GetParentElement(IUIAutomationElement element);
}

[ComImport, Guid("fb377fbe-8ea6-46d5-9c73-6499642d3059"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationInvokePattern
{
    void Invoke();
}

[ComImport, Guid("a94cd8b1-0844-4cd6-9d2d-640537ab39e9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationValuePattern
{
    void SetValue([MarshalAs(UnmanagedType.BStr)] string val);
    string CurrentValue { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsReadOnly { [return: MarshalAs(UnmanagedType.Bool)] get; }
}

[ComImport, Guid("94cf8058-9b8d-4ab9-8bfd-4cd0a33c8c70"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTogglePattern
{
    void Toggle();
    int CurrentToggleState { get; }
}

[ComImport, Guid("a8efa66a-0fda-421a-9194-38021f3578ea"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationSelectionItemPattern
{
    void Select();
    void AddToSelection();
    void RemoveFromSelection();
    bool CurrentIsSelected { [return: MarshalAs(UnmanagedType.Bool)] get; }
}

[ComImport, Guid("619be086-1f4e-4ee4-bafa-210128738730"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationExpandCollapsePattern
{
    void Expand();
    void Collapse();
    int CurrentExpandCollapseState { get; }
}

[ComImport, Guid("b488300f-d015-4f19-9c29-bb595e3645ef"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationScrollItemPattern
{
    void ScrollIntoView();
}

[ComImport, Guid("32eba289-3583-42c9-9c59-3b6d9a1e9b6a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTextPattern
{
    void Reserved_RangeFromPoint();
    void Reserved_RangeFromChild();
    void Reserved_GetSelection();
    void Reserved_GetVisibleRanges();
    IUIAutomationTextRange DocumentRange { get; }
}

[ComImport, Guid("a543cc6a-f4ae-494b-8239-c814481187a8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTextRange
{
    void Reserved_Clone();
    void Reserved_Compare();
    void Reserved_CompareEndpoints();
    void Reserved_ExpandToEnclosingUnit();
    void Reserved_FindAttribute();
    void Reserved_FindText();
    void Reserved_GetAttributeValue();
    void Reserved_GetBoundingRectangles();
    void Reserved_GetEnclosingElement();
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetText(int maxLength);
}

[ComImport, Guid("0faef453-9208-43ef-bbb2-3b485177864f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationWindowPattern
{
    void Close();
    [return: MarshalAs(UnmanagedType.Bool)]
    bool WaitForInputIdle(int milliseconds);
    void SetWindowVisualState(int state);
    bool CurrentCanMaximize { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentCanMinimize { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsModal { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsTopmost { [return: MarshalAs(UnmanagedType.Bool)] get; }
    int CurrentWindowVisualState { get; }
}

/// <summary>UIA property ids (UIA_*PropertyId in UIAutomationClient.h).</summary>
internal static class PropertyIds
{
    public const int ProcessId = 30002;
    public const int ControlType = 30003;
    public const int Name = 30005;
    public const int AutomationId = 30011;
    public const int ClassName = 30012;
    public const int IsExpandCollapsePatternAvailable = 30028;
    public const int IsInvokePatternAvailable = 30031;
    public const int IsScrollItemPatternAvailable = 30035;
    public const int IsSelectionItemPatternAvailable = 30036;
    public const int IsTextPatternAvailable = 30040;
    public const int IsTogglePatternAvailable = 30041;
    public const int IsValuePatternAvailable = 30043;
    public const int IsWindowPatternAvailable = 30044;
}

/// <summary>UIA pattern ids (UIA_*PatternId in UIAutomationClient.h).</summary>
internal static class PatternIds
{
    public const int Invoke = 10000;
    public const int Value = 10002;
    public const int ExpandCollapse = 10005;
    public const int Window = 10009;
    public const int SelectionItem = 10010;
    public const int Text = 10014;
    public const int Toggle = 10015;
    public const int ScrollItem = 10017;
}
