using WinUia.Core.Exceptions;
using WinUia.Core;
using WinUia.Examples.Winforms.Tests.Application;
using WinUia.Examples.Winforms.Tests.Extensions;

namespace WinUia.Examples.Winforms.Tests.Tests;

/// <summary>
/// Elements re-find themselves when their control is re-created: FindFirst results through their search, FindAll
/// results by identity. The example app's "Recreate" button replaces btnVolatile; "Reverse" re-creates the pnlItems buttons
/// in reverse order.
/// </summary>
[UiTest]
public sealed class SelfHealingTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = App.LaunchApplication();

    [TearDown]
    public void TearDown() => _mainForm.Dispose();

    private IReadOnlyList<Element> Items() => _mainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children);

    private IEnumerable<string> ItemNames() => Items().Select(i => i.Name);

    private void Recreate()
    {
        _mainForm.RecreateButton.Click();
        Eventually(() => _mainForm.VolatileButton.Name == "Volatile 2", "Recreate replaces the volatile button");
    }

    [Test]
    public void A_found_element_re_resolves_after_its_control_is_recreated()
    {
        var volatileButton = _mainForm.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));

        Recreate();

        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 2"));
    }

    [Test]
    public void FindAll_elements_re_resolve_to_the_same_control_when_siblings_move()
    {
        var items = Items();
        var namesBefore = items.Select(i => i.Name).ToArray();
        Assert.That(namesBefore, Is.EquivalentTo(["Item A", "Item B", "Item C"]));

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(namesBefore.Reverse()));

        Assert.That(items.Select(i => i.Name), Is.EqualTo(namesBefore));
    }

    [Test]
    public void A_FindAll_element_never_read_before_going_stale_reports_stale_instead_of_guessing()
    {
        var items = Items();

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().First() == "Item C");

        Assert.Throws<UiaStaleElementException>(() => _ = items[0].Name);
    }

    [Test]
    public void Without_stale_retries_a_recreated_control_reports_stale()
    {
        var volatileButton = _mainForm.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));
        Recreate();

        _mainForm.Context.StaleRetryCount = 0;

        Assert.Throws<UiaStaleElementException>(() => _ = volatileButton.Name);
    }

    [Test]
    public void An_element_without_a_locator_reports_stale_after_its_control_is_recreated()
    {
        var byHandle = _mainForm.Context.FromHandle(_mainForm.VolatileButton.NativeWindowHandle);
        Assert.That(byHandle.Name, Is.EqualTo("Volatile 1"));

        Recreate();

        Assert.Throws<UiaStaleElementException>(() => _ = byHandle.Name);
    }

    [Test]
    public void A_found_child_re_resolves_after_it_and_its_siblings_are_recreated()
    {
        var panel = _mainForm.ItemsPanel;
        var itemA = panel.Find(e => e.Name == "Item A", TreeScope.Children);
        Assert.That(itemA.Name, Is.EqualTo("Item A"));

        _mainForm.ReverseButton.Click();
        Eventually(() => ItemNames().First() == "Item C");

        Assert.That(itemA.Name, Is.EqualTo("Item A"));
    }
}
