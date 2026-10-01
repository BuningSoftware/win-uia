namespace WinUia.NUnit;

/// <summary>
/// A session-wide lock on the interactive desktop: one mouse, one keyboard focus, one foreground window.
/// Backed by a named semaphore, so it serialises UI tests across test processes (for example the parallel test
/// assemblies of <c>dotnet test MySolution.slnx</c>), and — unlike a mutex — it may be released from a different
/// thread than the one that took it.
/// </summary>
internal sealed class DesktopLock : IDisposable
{
    internal const string Name = @"Local\WinUia.Desktop";

    private readonly Semaphore _semaphore;
    private int _released;

    private DesktopLock(Semaphore semaphore) => _semaphore = semaphore;

    /// <summary>Waits up to <paramref name="timeout"/> for the desktop.</summary>
    public static DesktopLock Acquire(TimeSpan timeout)
    {
        var semaphore = new Semaphore(1, 1, Name);
        try
        {
            if (!semaphore.WaitOne(timeout))
                throw new TimeoutException($"Another process kept the desktop (UI test lock '{Name}') for more than {timeout}.");
        }
        catch
        {
            semaphore.Dispose();
            throw;
        }

        return new DesktopLock(semaphore);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
            return;
        _semaphore.Release();
        _semaphore.Dispose();
    }
}
