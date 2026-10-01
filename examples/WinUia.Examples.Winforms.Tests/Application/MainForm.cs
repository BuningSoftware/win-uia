using WinUia.Core;

namespace WinUia.Examples.Winforms.Tests.Application;

/// <summary>
/// Page object for the example app: the <see cref="App"/> itself, with the controls of its main window. Get it with
/// <c>App.LaunchApplication()</c> (<see cref="Extensions.AppExtensions"/>). The tab pages and the dialog are page objects
/// of their own, built on the elements they cover.
/// </summary>
public sealed class MainForm : App
{
    public Element Window => MainWindow;
    public Element Button => Window.FindByAutomationId("btnClick");
    public Element ResultLabel => Window.FindByAutomationId("lblResult");
    public Element InputBox => Window.FindByAutomationId("txtInput");
    public Element ToggleCheckBox => Window.FindByAutomationId("chkToggle");
    public Element ItemsList => Window.FindByAutomationId("lstItems");
    public Element RecreateButton => Window.FindByAutomationId("btnRecreate");
    public Element VolatileButton => Window.FindByAutomationId("btnVolatile");
    public Element ItemsPanel => Window.FindByAutomationId("pnlItems");
    public Element ReverseButton => Window.FindByAutomationId("btnReverse");
    public Element OpenDialogButton => Window.FindByAutomationId("btnOpenDialog");
    public Element TabControl => Window.FindByAutomationId("tabMain");

    public DialogForm OpenDialog()
    {
        OpenDialogButton.Click();
        return new DialogForm(FindWindow("WinUia Dialog"));
    }

    // The tab header (TabItem) and its page (Pane) share the tab's text, so ask for the TabItem explicitly.
    public Element TabHeader(string text) =>
        TabControl.Find(e => e.Name == text && e.ControlType == ControlType.TabItem, TreeScope.Children);

    public Tab1 SelectTab1()
    {
        TabHeader("Tab 1").Select();
        return new Tab1(Window);
    }

    public Tab2 SelectTab2()
    {
        TabHeader("Tab 2").Select();
        return new Tab2(Window);
    }
}
