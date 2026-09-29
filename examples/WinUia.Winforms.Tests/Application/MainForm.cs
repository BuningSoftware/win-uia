using System.Linq.Expressions;
using WinUia.Core;
using WinUia.Core.Elements;

namespace WinUia.Winforms.Tests.Application;

public class MainForm : Automation
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
        return new DialogForm(FindDialog("WinUia Dialog"));
    }

    // A modal dialog is a top-level window owned by the main window: UIA lists it under its owner or under the desktop.
    private Element FindDialog(string title)
    {
        var processId = Process.Id;
        Expression<Func<Element, bool>> isDialog = e => e.Name == title && e.ControlType == ControlType.Window && e.ProcessId == processId;
        var desktop = Context.GetRootElement();
        return Context.WaitFor(() => Window.TryFind(isDialog, TreeScope.Children, TimeSpan.Zero)
                                     ?? desktop.TryFind(isDialog, TreeScope.Children, TimeSpan.Zero))
               ?? throw new InvalidOperationException($"The dialog '{title}' did not appear.");
    }

    // The tab header (TabItem) and its page (Pane) share the tab's text, so ask for the TabItem explicitly.
    public Element TabHeader(string text) =>
        TabControl.Find(e => e.Name == text && e.ControlType == ControlType.TabItem, TreeScope.Children);

    public Tab1 SelectTab1() => SelectTab<Tab1>("Tab 1");
    public Tab2 SelectTab2() => SelectTab<Tab2>("Tab 2");

    private TTab SelectTab<TTab>(string header) where TTab : MainForm, new()
    {
        TabHeader(header).Select();
        return As<TTab>();
    }
}
