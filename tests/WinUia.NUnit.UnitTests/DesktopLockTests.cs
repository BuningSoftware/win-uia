namespace WinUia.NUnit.UnitTests;

public class DesktopLockTests
{
    [Test]
    public void Disposing_the_lock_releases_the_desktop()
    {
        var first = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        first.Dispose();
        first.Dispose(); // Idempotent: must not release twice.

        using var second = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        Assert.That(second, Is.Not.Null);
    }

    [Test]
    public void Acquire_times_out_while_another_holder_keeps_the_desktop()
    {
        using var holder = DesktopLock.Acquire(TimeSpan.FromMinutes(5));

        Exception? caught = null;
        var other = new Thread(() =>
        {
            try { using var _ = DesktopLock.Acquire(TimeSpan.FromMilliseconds(200)); }
            catch (Exception ex) { caught = ex; }
        });
        other.Start();
        other.Join();

        Assert.That(caught, Is.TypeOf<TimeoutException>());
    }
}
