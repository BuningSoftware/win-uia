using WinUia.Core.Elements;

namespace WinUia.Winforms.Tests.Application;

public sealed class Tab2 : MainForm
{
    public Element Content => Window.FindByAutomationId("lblTab2");
}
