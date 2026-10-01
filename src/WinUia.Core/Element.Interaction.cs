using WinUia.Core.Exceptions;
using WinUia.Input;
using WinUia.Core.Patterns;

namespace WinUia.Core;

public sealed partial class Element
{
    /// <summary>The Invoke pattern.</summary>
    public InvokePatternWrapper InvokePattern => new(this);

    /// <summary>The Value pattern.</summary>
    public ValuePatternWrapper ValuePattern => new(this);

    /// <summary>The Toggle pattern.</summary>
    public TogglePatternWrapper TogglePattern => new(this);

    /// <summary>The SelectionItem pattern.</summary>
    public SelectionItemPatternWrapper SelectionItemPattern => new(this);

    /// <summary>The ExpandCollapse pattern.</summary>
    public ExpandCollapsePatternWrapper ExpandCollapsePattern => new(this);

    /// <summary>The ScrollItem pattern.</summary>
    public ScrollItemPatternWrapper ScrollItemPattern => new(this);

    /// <summary>The Text pattern.</summary>
    public TextPatternWrapper TextPattern => new(this);

    /// <summary>The Window pattern.</summary>
    public WindowPatternWrapper WindowPattern => new(this);

    /// <summary>
    /// Clicks the element through the first pattern that means "click" for it: Invoke, Toggle, SelectionItem,
    /// then ExpandCollapse. Falls back to a physical mouse click (<see cref="PhysicalClick"/>).
    /// With <see cref="AutomationContext.ShowPointer"/> on, the cursor moves to the element first, as it does for the
    /// other interactions.
    /// <para>
    /// Native Win32 and WinForms buttons are clicked by posting <c>BM_CLICK</c> to their window instead of through
    /// the Invoke pattern. Their Invoke runs the click handler inside the UIA call, so a handler that shows a modal
    /// dialog would keep the application's UI thread inside that call until the dialog closes, and every other UIA
    /// request to the application would fail or time out meanwhile. A posted click runs from the application's own
    /// message loop, so the dialog can be automated like any other window.
    /// </para>
    /// </summary>
    public void Click()
    {
        EnsureEnabled();
        MovePointerHere();

        if (InvokePattern.IsSupported)
        {
            if (!TryPostNativeButtonClick())
                InvokePattern.Invoke();
        }
        else if (TogglePattern.IsSupported)
            TogglePattern.Toggle();
        else if (SelectionItemPattern.IsSupported)
            SelectionItemPattern.Select();
        else if (ExpandCollapsePattern.IsSupported)
            ToggleExpandCollapse();
        else
            PhysicalClick();
    }

    /// <summary>Posts BM_CLICK when the element is a native (Win32 "BUTTON" class) push button.</summary>
    private bool TryPostNativeButtonClick()
    {
        if (ControlType != ControlType.Button)
            return false;

        var hwnd = NativeWindowHandle;
        if (hwnd == 0 || !ClassName.Contains("BUTTON", StringComparison.OrdinalIgnoreCase))
            return false;

        return NativeButton.PostClick(hwnd);
    }

    /// <summary>Scrolls the element into view if possible and clicks its clickable point with the mouse.</summary>
    public void PhysicalClick(MouseButton button = MouseButton.Left)
    {
        EnsureEnabled();

        if (ScrollItemPattern.IsSupported)
            ScrollItemPattern.ScrollIntoView();

        var point = GetClickablePoint();
        if (Context.ShowPointer)
            Win32InputSimulator.MoveTo(point.X, point.Y, Context.PointerMoveDuration); // Instead of jumping there.
        Win32InputSimulator.ClickAt(point.X, point.Y, button);
    }

    /// <summary>
    /// A screen point that clicks the element: UIA's clickable point, or the centre of the bounding rectangle
    /// when UIA reports none but the element is visible.
    /// </summary>
    public ScreenPoint GetClickablePoint()
    {
        var (found, point) = PhysicalDpi.Run(() => Get(e => (e.GetClickablePoint(out var p), p)));
        if (found)
            return new ScreenPoint(point.x, point.y);

        var rect = BoundingRectangle;
        if (!rect.IsEmpty && !IsOffscreen)
            return rect.Center;

        throw new UiaNoClickablePointException($"{this} has no clickable point.");
    }

    /// <summary>
    /// Sets the element's text: through the Value pattern when it is available and writable, otherwise by
    /// focusing the element, selecting all and typing.
    /// </summary>
    public void SetValue(string value)
    {
        EnsureEnabled();
        MovePointerHere();

        if (ValuePattern.IsSupported && !ValuePattern.IsReadOnly)
        {
            ValuePattern.SetValue(value);
            return;
        }

        Focus();
        Win32InputSimulator.SendKeys(VirtualKey.Control, VirtualKey.A);
        if (value.Length == 0)
            Win32InputSimulator.SendKeys(VirtualKey.Delete);
        else
            Win32InputSimulator.SendText(value);
    }

    /// <summary>The element's text: Text pattern, then Value pattern, then Name.</summary>
    public string GetText()
    {
        if (TextPattern.IsSupported)
            return TextPattern.GetText();
        if (ValuePattern.IsSupported)
            return ValuePattern.Value;
        return Name;
    }

    /// <summary>Toggles the element (Toggle pattern).</summary>
    public void Toggle() => WhenEnabled(TogglePattern.Toggle);

    /// <summary>Selects the element (SelectionItem pattern).</summary>
    public void Select() => WhenEnabled(SelectionItemPattern.Select);

    /// <summary>Expands the element (ExpandCollapse pattern).</summary>
    public void Expand() => WhenEnabled(ExpandCollapsePattern.Expand);

    /// <summary>Collapses the element (ExpandCollapse pattern).</summary>
    public void Collapse() => WhenEnabled(ExpandCollapsePattern.Collapse);

    /// <summary>Gives the element keyboard focus.</summary>
    public void Focus()
    {
        MovePointerHere();
        Do(e => e.SetFocus());
    }

    /// <summary>
    /// With <see cref="AutomationContext.ShowPointer"/> on, moves the cursor to the element's clickable point over
    /// <see cref="AutomationContext.PointerMoveDuration"/>. Only shows where the interaction happens, so an element
    /// without a clickable point (off screen, collapsed) is simply not pointed at.
    /// </summary>
    private void MovePointerHere()
    {
        if (!Context.ShowPointer)
            return;

        ScreenPoint point;
        try
        {
            point = GetClickablePoint();
        }
        catch (UiaNoClickablePointException)
        {
            return;
        }

        Win32InputSimulator.MoveTo(point.X, point.Y, Context.PointerMoveDuration);
    }

    private void ToggleExpandCollapse()
    {
        if (ExpandCollapsePattern.State == ExpandCollapseState.Collapsed)
            ExpandCollapsePattern.Expand();
        else
            ExpandCollapsePattern.Collapse();
    }

    private void EnsureEnabled()
    {
        if (!IsEnabled)
            throw new UiaElementNotEnabledException($"{this} is not enabled.");
    }

    private void WhenEnabled(Action action)
    {
        EnsureEnabled();
        MovePointerHere();
        action();
    }
}
