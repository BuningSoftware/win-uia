using System.Diagnostics;
using WinUia.Core.Exceptions;
using WinUia.Testing.Shared;

namespace WinUia.UnitTests;

/// <summary>
/// Behaviour of <see cref="App"/> itself: main window, Find timeouts, closing and ownership, against the empty test app.
/// Finding controls and windows by content is tested in the WinForms example.
/// </summary>
[UiTest]
public sealed class AppTests
{
    private App _app = null!;

    [SetUp]
    public void LaunchApp() => _app = App.Launch(AppPaths.TestApp);

    [TearDown]
    public void CloseApp() => _app.Dispose();

    [Test]
    public void Launch_finds_the_main_window()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.MainWindow.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(_app.MainWindow.ProcessId, Is.EqualTo(_app.Process.Id));
        }
    }

    [Test]
    public void Find_of_a_missing_element_throws_after_the_timeout_and_TryFind_returns_null()
    {
        _ = _app.MainWindow; // The first Find also waits for the window to appear; keep that out of the timing.

        var stopwatch = Stopwatch.StartNew();
        Assert.Throws<UiaElementNotFoundException>(() => _app.Find("doesNotExist", TimeSpan.FromMilliseconds(300)));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(290, 5000));

        Assert.That(_app.TryFind("doesNotExist", TimeSpan.FromMilliseconds(100)), Is.Null);
    }

    [Test]
    public void Close_ends_the_process()
    {
        _app.Close();

        Assert.That(_app.Process.HasExited, Is.True);
    }

    [Test]
    public void Dispose_closes_a_launched_app_but_not_an_attached_one()
    {
        using (var attached = App.Attach(_app.Process.Id))
        {
            Assert.That(attached.MainWindow.Name, Is.EqualTo("WinUia Test App"));
        }

        Assert.That(_app.Process.HasExited, Is.False);

        var pid = _app.Process.Id;
        _app.Dispose();
        Assert.Throws<ArgumentException>(() => Process.GetProcessById(pid));
    }

    [Test]
    public void As_hands_the_app_over_to_a_derived_page_object()
    {
        var original = _app;
        var derived = original.As<DerivedApp>();
        _app = derived; // TearDown disposes the instance that owns the app now.

        original.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(derived.Process.HasExited, Is.False, "disposing the handed-over original leaves the app running");
            Assert.That(derived.Title, Is.EqualTo("WinUia Test App"));
            Assert.Throws<ObjectDisposedException>(() => original.As<DerivedApp>());
        }

        var pid = derived.Process.Id;
        derived.Dispose();
        Assert.Throws<ArgumentException>(() => Process.GetProcessById(pid));
    }

    [Test]
    public void As_twice_on_the_same_app_throws()
    {
        var derived = _app.As<DerivedApp>();
        var original = _app;
        _app = derived;

        Assert.Throws<InvalidOperationException>(() => original.As<DerivedApp>());
    }

    [Test]
    public void A_derived_app_created_directly_is_not_connected()
    {
        using var unconnected = new DerivedApp();

        Assert.Throws<InvalidOperationException>(() => _ = unconnected.Process);
    }

    [Test]
    public void Attaching_to_a_missing_process_throws_an_AppProcessException()
    {
        Assert.Throws<AppProcessException>(() => App.Attach("WinUia.No.Such.Process"));
    }
}
