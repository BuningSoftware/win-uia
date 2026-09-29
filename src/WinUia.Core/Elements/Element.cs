using System.Runtime.InteropServices;
using WinUia.Core.Exceptions;
using WinUia.Core.Interop;
using WinUia.Input;

namespace WinUia.Core.Elements;

/// <summary>
/// A UI Automation element. Elements found through a search remember how they were found, and every call
/// re-finds the element and retries when UIA reports it as stale (for example after a WinUI 3 re-render).
/// </summary>
public sealed partial class Element
{
    private ElementIdentity? _identity;

    internal Element(AutomationContext context, IUIAutomationElement raw, ElementLocator? locator)
    {
        Context = context;
        Raw = raw;
        Locator = locator;
    }

    /// <summary>The context this element belongs to.</summary>
    public AutomationContext Context { get; }

    internal ElementLocator? Locator { get; }

    internal IUIAutomationElement Raw { get; private set; }

    /// <summary>The element's Name.</summary>
    public string Name => Get(e => e.CurrentName) ?? "";

    /// <summary>The element's AutomationId.</summary>
    public string AutomationId => Get(e => e.CurrentAutomationId) ?? "";

    /// <summary>The element's ClassName.</summary>
    public string ClassName => Get(e => e.CurrentClassName) ?? "";

    /// <summary>The element's FrameworkId (for example "Win32", "WinForm", "WPF", "XAML").</summary>
    public string FrameworkId => Get(e => e.CurrentFrameworkId) ?? "";

    /// <summary>The element's control type.</summary>
    public ControlType ControlType => (ControlType)Get(e => e.CurrentControlType);

    /// <summary>The id of the process that owns the element.</summary>
    public int ProcessId => Get(e => e.CurrentProcessId);

    /// <summary>Whether the element is enabled.</summary>
    public bool IsEnabled => Get(e => e.CurrentIsEnabled);

    /// <summary>Whether the element is scrolled or hidden out of view.</summary>
    public bool IsOffscreen => Get(e => e.CurrentIsOffscreen);

    /// <summary>Whether the element has keyboard focus.</summary>
    public bool HasKeyboardFocus => Get(e => e.CurrentHasKeyboardFocus);

    /// <summary>The native window handle, or 0 when the element is not a window.</summary>
    public nint NativeWindowHandle => Get(e => e.CurrentNativeWindowHandle);

    /// <summary>The element's bounding rectangle in physical screen pixels.</summary>
    public ScreenRect BoundingRectangle
    {
        get
        {
            var r = PhysicalDpi.Run(() => Get(e => e.CurrentBoundingRectangle));
            return new ScreenRect(r.left, r.top, r.right, r.bottom);
        }
    }

    /// <summary>The UIA runtime id, which identifies the element for its lifetime.</summary>
    public IReadOnlyList<int> RuntimeId => Get(e => e.GetRuntimeId()) ?? [];

    /// <summary>Reads any UIA property by id (UIA_*PropertyId). Returns null when the property is empty.</summary>
    public object? GetPropertyValue(int propertyId) => Get(e => e.GetCurrentPropertyValue(propertyId));

    /// <summary>Re-finds the element through the search it came from. Returns false when that is not possible.</summary>
    public bool Refresh()
    {
        if (Locator is null)
            return false;

        var fresh = Context.WaitFor(() => Locator.ResolveOnce(Context, _identity), Context.StaleRefreshTimeout);
        if (fresh is null)
            return false;

        Raw = fresh;
        return true;
    }

    /// <summary>Whether both elements refer to the same UI element.</summary>
    public bool IsSameAs(Element other) =>
        Get(e => other.Get(o => Context.Automation.CompareElements(e, o)));

    /// <summary>Runs a call on the raw element, re-resolving and retrying if it is stale.</summary>
    internal T Get<T>(Func<IUIAutomationElement, T> call)
    {
        AutomationContext.EnsureMta();
        return Retry.OnStale(() =>
        {
            var result = call(Raw);
            CaptureIdentity();
            return result;
        }, Refresh, Context.StaleRetryCount);
    }

    /// <summary>
    /// FindAll results re-resolve by identity, not by position, so remember what this control is the first time
    /// it answers a call. One that goes stale before it was ever used cannot be re-resolved and reports stale.
    /// </summary>
    private void CaptureIdentity()
    {
        if (_identity is not null || Locator?.Index is null)
            return;

        try
        {
            _identity = ElementIdentity.From(Raw);
        }
        catch (COMException)
        {
            // Went stale right after the call; the next call reports it.
        }
    }

    internal void Do(Action<IUIAutomationElement> call) =>
        Get(e => { call(e); return true; });

    /// <inheritdoc />
    public override string ToString()
    {
        try
        {
            return $"{ControlType} \"{Name}\" (AutomationId=\"{AutomationId}\")";
        }
        catch (UiaException)
        {
            return $"<unavailable element: {Locator}>";
        }
    }
}
