using WinUia.Core.Interop;

namespace WinUia.Core.Patterns;

/// <summary>The ScrollItem pattern: items inside a scrollable container.</summary>
public sealed class ScrollItemPatternWrapper : PatternWrapper
{
    internal ScrollItemPatternWrapper(Element element)
        : base(element, PatternIds.ScrollItem, PropertyIds.IsScrollItemPatternAvailable, "ScrollItem") { }

    /// <summary>Scrolls the container so the item is visible.</summary>
    public void ScrollIntoView() => Call<IUIAutomationScrollItemPattern>(p => p.ScrollIntoView());
}
