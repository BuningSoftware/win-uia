using WinUia.Testing.Shared;
using WinUia.Winforms.Tests.Application;
using WinUia.Winforms.Tests.Extensions;

namespace WinUia.Winforms.Tests.Tests;

[UiTest]
public sealed class TabTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = Automation.LaunchApplication(TestAppPath.Exe);

    [Test]
    public void Tab_1_is_selected_at_start()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.TabHeader("Tab 1").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(_mainForm.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.False);
        }
    }

    [Test]
    public void SelectTab1_shows_the_tab_1_content()
    {
        var tab1 = _mainForm.SelectTab1();

        Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
    }

    [Test]
    public void SelectTab2_selects_the_tab_and_shows_the_tab_2_content()
    {
        var tab2 = _mainForm.SelectTab2();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [Test]
    public void Switching_tabs_hides_the_other_tabs_content()
    {
        _mainForm.SelectTab2();
        var tab1 = _mainForm.SelectTab1();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
            Assert.That(tab1.Window.TryFindByAutomationId("lblTab2", TimeSpan.FromMilliseconds(200)), Is.Null);
        }
    }

    [Test]
    public void A_tab_is_the_same_application_and_keeps_the_main_form_controls()
    {
        var tab2 = _mainForm.SelectTab2();

        tab2.Button.Click();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tab2.Process.Id, Is.EqualTo(_mainForm.Process.Id));
            Eventually(() => _mainForm.ResultLabel.Name == "Clicked");
        }
    }

    [Test]
    public void Disposing_a_tab_leaves_the_application_running()
    {
        using (_mainForm.SelectTab2())
        {
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_mainForm.Process.HasExited, Is.False);
            Assert.That(_mainForm.SelectTab1().Content.Name, Is.EqualTo("tab1 content"));
        }
    }

    [TearDown]
    public void TearDown() => _mainForm.Dispose();
}
