using WinUia.Examples.Winforms.Tests.Application;
using WinUia.Examples.Winforms.Tests.Extensions;

namespace WinUia.Examples.Winforms.Tests.Tests;

[UiTest]
public sealed class TabTests
{
    private MainForm _mainForm = null!;

    [SetUp]
    public void SetUp() => _mainForm = App.LaunchApplication();

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
            Assert.That(_mainForm.Window.TryFindByAutomationId("lblTab2", TimeSpan.FromMilliseconds(200)), Is.Null);
        }
    }

    [Test]
    public void The_main_form_controls_keep_working_while_another_tab_is_selected()
    {
        var tab2 = _mainForm.SelectTab2();

        _mainForm.Button.Click();

        using (Assert.EnterMultipleScope())
        {
            Eventually(() => _mainForm.ResultLabel.Name == "Clicked");
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [TearDown]
    public void TearDown() => _mainForm.Dispose();
}
