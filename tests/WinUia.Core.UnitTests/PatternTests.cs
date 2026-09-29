using WinUia.Core.Elements;
using WinUia.Core.Exceptions;
using WinUia.Core.Patterns;
using WinUia.Input;

namespace WinUia.Core.UnitTests;

[UiTest]
public sealed class PatternTests
{
    private TestAppSession _app = null!;

    [SetUp]
    public void LaunchApp() => _app = new TestAppSession();

    [TearDown]
    public void CloseApp() => _app.Dispose();

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hwnd, int index);

    private static bool IsTopmostWindow(nint hwnd) => (GetWindowLongPtr(hwnd, -20 /* GWL_EXSTYLE */) & 0x8 /* WS_EX_TOPMOST */) != 0;

    [Test]
    public void Fixture_elements_have_aligned_vtables()
    {
        InteropVtableTests.AssertTypedGettersMatchPropertyValues(_app.Window);
        InteropVtableTests.AssertTypedGettersMatchPropertyValues(_app.Get("btnClick"));
        InteropVtableTests.AssertTypedGettersMatchPropertyValues(_app.Get("txtInput"));
    }

    [Test]
    public void Click_invokes_a_button()
    {
        _app.Get("btnClick").Click();

        Eventually(() => _app.Get("lblResult").Name == "Clicked", "the button click handler sets the label");
    }

    [Test]
    public void SetValue_uses_the_value_pattern()
    {
        var input = _app.Get("txtInput");

        input.SetValue("hello");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(input.ValuePattern.Value, Is.EqualTo("hello"));
            Assert.That(input.ValuePattern.IsReadOnly, Is.False);
        }
    }

    [Test]
    public void Toggle_flips_a_checkbox()
    {
        var toggle = _app.Get("chkToggle");
        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.Off));

        toggle.Toggle();
        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.On));

        toggle.Click();
        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.Off));
    }

    [Test]
    public void Select_selects_a_list_item()
    {
        var item = _app.Get("lstItems").Find(e => e.Name == "Beta");

        item.Select();

        Assert.That(item.SelectionItemPattern.IsSelected, Is.True);
    }

    [Test]
    public void Unsupported_pattern_throws()
    {
        var label = _app.Get("lblResult");

        Assert.That(label.InvokePattern.IsSupported, Is.False);
        Assert.Throws<UiaPatternNotSupportedException>(() => label.InvokePattern.Invoke());
    }

    [Test]
    public void Window_pattern_reports_window_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.Window.WindowPattern.IsSupported, Is.True);
            Assert.That(_app.Window.WindowPattern.VisualState, Is.EqualTo(WindowVisualState.Normal));
        }
    }

    [Test]
    public void GetText_reads_the_value_of_an_edit()
    {
        var input = _app.Get("txtInput");
        input.SetValue("some text");

        Assert.That(input.GetText(), Is.EqualTo("some text"));
    }

    [Test]
    public void Clickable_point_is_inside_the_bounding_rectangle()
    {
        var button = _app.Get("btnClick");

        var point = button.GetClickablePoint();
        var rect = button.BoundingRectangle;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.X, Is.InRange(rect.Left, rect.Right));
            Assert.That(point.Y, Is.InRange(rect.Top, rect.Bottom));
        }
    }

    [Test]
    public void Composite_conditions_search_correctly()
    {
        var both = _app.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var either = _app.Window.Find(e => e.AutomationId == "nope" || e.AutomationId == "chkToggle");
        var notButton = _app.Window.FindAll(e => !(e.ControlType == ControlType.Button), TreeScope.Children);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(both.AutomationId, Is.EqualTo("btnClick"));
            Assert.That(either.AutomationId, Is.EqualTo("chkToggle"));
            Assert.That(notButton, Has.None.Matches<Element>(e => e.ControlType == ControlType.Button));
        }
    }

    [Test]
    public void Predicate_searches_find_the_expected_elements()
    {
        var button = _app.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var items = _app.Get("lstItems").FindAll(e => e.ControlType == ControlType.ListItem && e.Name != "Beta", TreeScope.Children);
        var missing = _app.Window.TryFind(e => e.Name == "nope", timeout: TimeSpan.Zero);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.AutomationId, Is.EqualTo("btnClick"));
            Assert.That(items.Select(i => i.Name), Is.EqualTo(["Alpha", "Gamma"]));
            Assert.That(missing, Is.Null);
        }
    }

    [Test]
    public void FromPoint_and_GetFocusedElement_return_the_expected_elements()
    {
        var input = _app.Get("txtInput");
        input.Focus();

        Eventually(() => _app.Context.GetFocusedElement().AutomationId == "txtInput", "SetFocus moves keyboard focus");
        Assert.That(_app.Context.FromPoint(_app.Get("btnClick").GetClickablePoint()).AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void Parent_and_Ancestors_walk_up_to_the_root()
    {
        var button = _app.Get("btnClick");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Parent!.IsSameAs(_app.Window), Is.True);
            Assert.That(button.Ancestors().Last().IsSameAs(_app.Context.GetRootElement()), Is.True);
        }
    }

    [Test]
    public void AddToSelection_and_RemoveFromSelection_change_a_multi_select_list()
    {
        var list = _app.Get("lstItems");
        var alpha = list.Find(e => e.Name == "Alpha");
        var gamma = list.Find(e => e.Name == "Gamma");

        alpha.Select();
        gamma.SelectionItemPattern.AddToSelection();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(alpha.SelectionItemPattern.IsSelected, Is.True);
            Assert.That(gamma.SelectionItemPattern.IsSelected, Is.True);
        }

        alpha.SelectionItemPattern.RemoveFromSelection();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(alpha.SelectionItemPattern.IsSelected, Is.False);
            Assert.That(gamma.SelectionItemPattern.IsSelected, Is.True);
        }
    }

    [Test]
    public void Window_pattern_members_work()
    {
        var window = _app.Window.WindowPattern;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.CanMaximize, Is.True);
            Assert.That(window.CanMinimize, Is.True);
            Assert.That(window.IsModal, Is.False);
            // Compare with Win32 rather than assume the fixture's TopMost: it is the vtable slot under test here.
            Assert.That(window.IsTopmost, Is.EqualTo(IsTopmostWindow(_app.Window.NativeWindowHandle)));
        }

        // The WinForms provider returns E_NOTIMPL for WaitForInputIdle.
        Assert.Throws<UiaPatternNotSupportedException>(() => window.WaitForInputIdle(TimeSpan.FromSeconds(5)));

        window.SetVisualState(WindowVisualState.Maximized);
        Eventually(() => window.VisualState == WindowVisualState.Maximized, "the window maximizes");
        window.SetVisualState(WindowVisualState.Normal);
        Eventually(() => window.VisualState == WindowVisualState.Normal, "the window restores");
    }

    [Test]
    public void Physical_click_and_typing_reach_the_app()
    {
        var input = _app.Get("txtInput");

        input.PhysicalClick();
        Eventually(() => input.HasKeyboardFocus, "a physical click focuses the text box");

        Win32InputSimulator.SendText("typed");
        Eventually(() => input.ValuePattern.Value == "typed", "SendText types into the focused text box");
    }
}
