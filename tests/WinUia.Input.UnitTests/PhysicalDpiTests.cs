using System.Runtime.InteropServices;

namespace WinUia.Input.UnitTests;

public class PhysicalDpiTests
{
    private const int DPI_AWARENESS_PER_MONITOR_AWARE = 2;

    [DllImport("user32.dll")]
    private static extern nint GetThreadDpiAwarenessContext();

    [DllImport("user32.dll")]
    private static extern int GetAwarenessFromDpiAwarenessContext(nint value);

    private static int CurrentAwareness() => GetAwarenessFromDpiAwarenessContext(GetThreadDpiAwarenessContext());

    [Test]
    public void Run_is_per_monitor_aware_inside_and_restores_the_thread_context_afterwards()
    {
        var before = CurrentAwareness();

        var inside = PhysicalDpi.Run(CurrentAwareness);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(inside, Is.EqualTo(DPI_AWARENESS_PER_MONITOR_AWARE));
            Assert.That(CurrentAwareness(), Is.EqualTo(before));
        }
    }

    [Test]
    public void Run_restores_the_thread_context_when_the_action_throws()
    {
        var before = CurrentAwareness();

        Assert.Throws<InvalidOperationException>(() => PhysicalDpi.Run(() => throw new InvalidOperationException()));

        Assert.That(CurrentAwareness(), Is.EqualTo(before));
    }
}
