using WinUia.Core;
using WinUia.Core.Exceptions;
using WinUia.Core.Patterns;
using WinUia.Testing.Shared;
using WinUia.Winforms.Tests.Application;
using WinUia.Winforms.Tests.Extensions;

namespace WinUia.Winforms.Tests.Tests;

[UiTest]
public sealed class MainFormTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = Automation.LaunchApplication(TestAppPath.Exe);
    
    [Test]
    public void Launch_returns_the_main_form()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.Window.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(_mainForm.Window.ProcessId, Is.EqualTo(_mainForm.Process.Id));
        }
    }

    [Test]
    public void The_form_starts_in_its_initial_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.ResultLabel.Name, Is.EqualTo("Ready"));
            Assert.That(_mainForm.InputBox.ValuePattern.Value, Is.Empty);
            Assert.That(_mainForm.ToggleCheckBox.TogglePattern.State, Is.EqualTo(ToggleState.Off));
            Assert.That(_mainForm.VolatileButton.Name, Is.EqualTo("Volatile 1"));
            Assert.That(ItemNames(), Is.EqualTo(["Item A", "Item B", "Item C"]));
            Assert.That(_mainForm.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem, TreeScope.Children).Select(i => i.Name), Is.EqualTo(["Alpha", "Beta", "Gamma"]));
        }
    }

    [Test]
    public void Selecting_a_list_item_selects_it()
    {
        var beta = _mainForm.ItemsList.Find(e => e.Name == "Beta");

        beta.Select();

        Assert.That(beta.SelectionItemPattern.IsSelected, Is.True);
    }

    [Test]
    public void Recreate_increments_the_volatile_button_generation()
    {
        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 2");

        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 3");
    }

    [Test]
    public void Reverse_twice_restores_the_item_order()
    {
        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item C", "Item B", "Item A"]));

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item A", "Item B", "Item C"]));
    }

    [Test]
    public void Clicking_the_button_updates_the_label()
    {
        _mainForm.Button.Click();

        Eventually(() => _mainForm.ResultLabel.Name == "Clicked");
    }

    [Test]
    public void SetValue_round_trips()
    {
        var input = _mainForm.InputBox;

        input.SetValue("round trip");

        Assert.That(input.ValuePattern.Value, Is.EqualTo("round trip"));
    }

    [Test]
    public void Toggle_changes_the_checkbox()
    {
        var toggle = _mainForm.ToggleCheckBox;

        toggle.Toggle();

        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.On));
    }

    [Test]
    public void A_found_element_re_resolves_after_its_control_is_recreated()
    {
        var volatileButton = _mainForm.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));

        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 2");

        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 2"));
    }

    [Test]
    public void FindAll_elements_re_resolve_to_the_same_control_when_siblings_move()
    {
        var items = _mainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children);
        var namesBefore = items.Select(i => i.Name).ToArray();
        Assert.That(namesBefore, Is.EquivalentTo(["Item A", "Item B", "Item C"]));

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(namesBefore.Reverse()));

        Assert.That(items.Select(i => i.Name), Is.EqualTo(namesBefore));
    }

    [Test]
    public void A_FindAll_element_never_read_before_going_stale_reports_stale_instead_of_guessing()
    {
        var items = _mainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children);

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().First() == "Item C");

        Assert.Throws<UiaStaleElementException>(() => _ = items[0].Name);
    }

    [Test]
    public void Without_stale_retries_a_recreated_control_reports_stale()
    {
        var volatileButton = _mainForm.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));
        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 2");

        _mainForm.Context.StaleRetryCount = 0;

        Assert.Throws<UiaStaleElementException>(() => _ = volatileButton.Name);
    }

    [Test]
    public void An_element_without_a_locator_reports_stale_after_its_control_is_recreated()
    {
        var byHandle = _mainForm.Context.FromHandle(_mainForm.VolatileButton.NativeWindowHandle);
        Assert.That(byHandle.Name, Is.EqualTo("Volatile 1"));

        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 2");

        Assert.Throws<UiaStaleElementException>(() => _ = byHandle.Name);
    }
    
    [TearDown]
    public void TearDown() => _mainForm.Dispose();

    private IEnumerable<string> ItemNames() => _mainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children).Select(i => i.Name);
}
