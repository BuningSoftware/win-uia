using System.Diagnostics;
using WinUia.Core.Exceptions;
using WinUia.Testing.Shared;

namespace WinUia.UnitTests;

/// <summary>Behaviour of <see cref="Automation"/> itself: main window, Find semantics, closing and ownership.</summary>
[UiTest]
public sealed class AutomationTests
{
    private Automation _automation = null!;

    [SetUp]
    public void LaunchApp() => _automation = Automation.Launch(TestAppPath.Exe);

    [TearDown]
    public void CloseApp() => _automation.Dispose();

    [Test]
    public void Launch_finds_the_main_window()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_automation.MainWindow.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(_automation.MainWindow.ProcessId, Is.EqualTo(_automation.Process.Id));
        }
    }

    [Test]
    public void Find_falls_back_to_the_name()
    {
        Assert.That(_automation.Find("Click me").AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void Find_of_a_missing_element_throws_after_the_timeout_and_TryFind_returns_null()
    {
        _ = _automation.MainWindow; // The first Find also waits for the window to appear; keep that out of the timing.

        var stopwatch = Stopwatch.StartNew();
        Assert.Throws<UiaElementNotFoundException>(() => _automation.Find("doesNotExist", TimeSpan.FromMilliseconds(300)));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(290, 5000));

        Assert.That(_automation.TryFind("doesNotExist", TimeSpan.FromMilliseconds(100)), Is.Null);
    }

    [Test]
    public void Close_ends_the_process()
    {
        _automation.Close();

        Assert.That(_automation.Process.HasExited, Is.True);
    }

    [Test]
    public void Dispose_closes_a_launched_app_but_not_an_attached_one()
    {
        using (var attached = Automation.Attach(_automation.Process.Id))
        {
            Assert.That(attached.MainWindow.Name, Is.EqualTo("WinUia Test App"));
        }

        Assert.That(_automation.Process.HasExited, Is.False);

        var pid = _automation.Process.Id;
        _automation.Dispose();
        Assert.Throws<ArgumentException>(() => Process.GetProcessById(pid));
    }
}
