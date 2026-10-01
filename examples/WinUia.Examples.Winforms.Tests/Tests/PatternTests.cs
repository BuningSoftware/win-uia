using WinUia.Core.Exceptions;
using WinUia.Core.Patterns;
using WinUia.Core;
using WinUia.Examples.Winforms.Tests.Application;
using WinUia.Examples.Winforms.Tests.Extensions;
using WinUia.Input;

namespace WinUia.Examples.Winforms.Tests.Tests;

/// <summary>Control patterns, searches, tree navigation and physical input, exercised on the example app's controls.</summary>

[UiTest]
public sealed class PatternTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = App.LaunchApplication();

    [TearDown]
    public void TearDown() => _mainForm.Dispose();

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hwnd, int index);

    private static bool IsTopmostWindow(nint hwnd) => (GetWindowLongPtr(hwnd, -20 /* GWL_EXSTYLE */) & 0x8 /* WS_EX_TOPMOST */) != 0;

    [Test]
    public void Unsupported_pattern_throws()
    {
        var label = _mainForm.ResultLabel;

        Assert.That(label.InvokePattern.IsSupported, Is.False);
        Assert.Throws<UiaPatternNotSupportedException>(() => label.InvokePattern.Invoke());
    }

    [Test]
    public void Window_pattern_reports_window_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.Window.WindowPattern.IsSupported, Is.True);
            Assert.That(_mainForm.Window.WindowPattern.VisualState, Is.EqualTo(WindowVisualState.Normal));
        }
    }

    [Test]
    public void GetText_reads_the_value_of_an_edit()
    {
        var input = _mainForm.InputBox;
        input.SetValue("some text");

        Assert.That(input.GetText(), Is.EqualTo("some text"));
    }

    [Test]
    public void Clickable_point_is_inside_the_bounding_rectangle()
    {
        var button = _mainForm.Button;

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
        var both = _mainForm.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var either = _mainForm.Window.Find(e => e.AutomationId == "nope" || e.AutomationId == "chkToggle");
        var notButton = _mainForm.Window.FindAll(e => !(e.ControlType == ControlType.Button), TreeScope.Children);

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
        var button = _mainForm.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var items = _mainForm.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem && e.Name != "Beta", TreeScope.Children);
        var missing = _mainForm.Window.TryFind(e => e.Name == "nope", timeout: TimeSpan.Zero);

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
        var input = _mainForm.InputBox;
        input.Focus();

        Eventually(() => _mainForm.Context.GetFocusedElement().AutomationId == "txtInput", "SetFocus moves keyboard focus");
        Assert.That(_mainForm.Context.FromPoint(_mainForm.Button.GetClickablePoint()).AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void Parent_and_Ancestors_walk_up_to_the_root()
    {
        var button = _mainForm.Button;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Parent!.IsSameAs(_mainForm.Window), Is.True);
            Assert.That(button.Ancestors().Last().IsSameAs(_mainForm.Context.GetRootElement()), Is.True);
        }
    }

    [Test]
    public void AddToSelection_and_RemoveFromSelection_change_a_multi_select_list()
    {
        var list = _mainForm.ItemsList;
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
        var window = _mainForm.Window.WindowPattern;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.CanMaximize, Is.True);
            Assert.That(window.CanMinimize, Is.True);
            Assert.That(window.IsModal, Is.False);
            // Compare with Win32 rather than assume the fixture's TopMost: it is the vtable slot under test here.
            Assert.That(window.IsTopmost, Is.EqualTo(IsTopmostWindow(_mainForm.Window.NativeWindowHandle)));
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
        var input = _mainForm.InputBox;

        input.PhysicalClick();
        Eventually(() => input.HasKeyboardFocus, "a physical click focuses the text box");

        Win32InputSimulator.SendText("typed");
        Eventually(() => input.ValuePattern.Value == "typed", "SendText types into the focused text box");
    }
}
