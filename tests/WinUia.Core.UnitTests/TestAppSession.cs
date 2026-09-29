using System.Diagnostics;
using WinUia.Core.Elements;
using WinUia.Testing.Shared;

namespace WinUia.Core.UnitTests;

/// <summary>Launches WinUia.Winforms for one test and kills it afterwards.</summary>
internal sealed class TestAppSession : IDisposable
{
    public TestAppSession()
    {
        Context = new AutomationContext();
        Process = Process.Start(TestAppPath.Exe);
        var processId = Process.Id;
        Window = Context.GetRootElement().Find(
            e => e.ProcessId == processId && e.ControlType == ControlType.Window,
            TreeScope.Children,
            TimeSpan.FromSeconds(15));
    }

    public AutomationContext Context { get; }
    public Process Process { get; }
    public Element Window { get; }

    public Element Get(string automationId) => Window.FindByAutomationId(automationId);

    public void Dispose()
    {
        if (!Process.HasExited)
        {
            Process.Kill(entireProcessTree: true);
            Process.WaitForExit(5000);
        }

        Process.Dispose();
        Context.Dispose();
    }
}
