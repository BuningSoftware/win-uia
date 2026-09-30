using System.Diagnostics;

namespace WinUia.Core;

/// <summary>
/// Polling, the way WinUia waits for UI state: <see cref="AutomationContext.WaitFor{T}"/> polls through it, and so can
/// test assertions that wait for asynchronous UI updates.
/// </summary>
public static class Poll
{
    /// <summary>How long a wait lasts unless told otherwise: 5 seconds (<see cref="AutomationContext.DefaultTimeout"/>).</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>How often a wait probes unless told otherwise: 100 ms (<see cref="AutomationContext.PollingInterval"/>).</summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Calls <paramref name="probe"/> every <paramref name="interval"/> until it returns non-null, and returns that
    /// result; returns null when <paramref name="timeout"/> elapses first. The probe runs at least once, so a zero
    /// timeout means "look now".
    /// </summary>
    public static T? Until<T>(Func<T?> probe, TimeSpan timeout, TimeSpan interval) where T : class
    {
        var waited = Stopwatch.StartNew();
        while (true)
        {
            if (probe() is { } result)
                return result;

            var remaining = timeout - waited.Elapsed;
            if (remaining <= TimeSpan.Zero)
                return null;

            Thread.Sleep(remaining < interval ? remaining : interval);
        }
    }

    /// <summary>Like <see cref="Until{T}"/> for a condition: true as soon as it holds, false when the timeout elapses first.</summary>
    public static bool Until(Func<bool> condition, TimeSpan timeout, TimeSpan interval) =>
        Until(() => condition() ? Holds : null, timeout, interval) is not null;

    private static readonly object Holds = new();
}
