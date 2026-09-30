using WinUia.Core;
using WinUia.Core.Patterns;
using WinUia.Examples.Winforms.Tests.Application;
using WinUia.Examples.Winforms.Tests.Extensions;

namespace WinUia.Examples.Winforms.Tests.Tests;

[UiTest]
public sealed class MainFormTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = App.LaunchApplication();
    
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
    public void Find_falls_back_to_the_name()
    {
        Assert.That(_mainForm.Find("Click me").AutomationId, Is.EqualTo("btnClick"));
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

    [TearDown]
    public void TearDown() => _mainForm.Dispose();

    private IEnumerable<string> ItemNames() => _mainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children).Select(i => i.Name);
}
