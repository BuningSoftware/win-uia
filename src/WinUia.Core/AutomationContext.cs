using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using WinUia.Core.Elements;
using WinUia.Core.Interop;
using WinUia.Input;

namespace WinUia.Core;

/// <summary>
/// Owns the UI Automation client object and the settings used by every element created from it.
/// Must be created on an MTA thread: UIA client calls from an STA thread that pumps messages can deadlock
/// against the automated application.
/// </summary>
public sealed class AutomationContext : IDisposable
{
    private IUIAutomation? _automation;

    /// <summary>Creates the UIA client. Throws <see cref="InvalidOperationException"/> on an STA thread.</summary>
    public AutomationContext()
    {
        EnsureMta();

        // UIA reports coordinates relative to the caller's DPI awareness, and SendInput uses the same space.
        // Coordinate reads and input run in a per-monitor-v2 thread scope (PhysicalDpi); making the process
        // per-monitor aware as well covers UIA builds that only honour process awareness.
        PhysicalDpi.MakeProcessAware();

        _automation = (IUIAutomation)new CUIAutomation8();
    }

    /// <summary>How long <c>Find</c> methods wait for an element. Default 5 seconds.</summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>How often waits poll. Default 100 ms.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>How many times a call on a stale element is retried after re-resolving it. Default 3.</summary>
    public int StaleRetryCount { get; set; } = 3;

    /// <summary>How long re-resolving a stale element waits for it to reappear. Default 1 second.</summary>
    public TimeSpan StaleRefreshTimeout { get; set; } = TimeSpan.FromSeconds(1);

    internal IUIAutomation Automation => _automation ?? throw new ObjectDisposedException(nameof(AutomationContext));

    /// <summary>The desktop (root) element.</summary>
    public Element GetRootElement() =>
        new(this, ComExceptionMapper.Invoke(() => Automation.GetRootElement()), ElementLocator.Root);

    /// <summary>The element for a native window handle.</summary>
    public Element FromHandle(nint hwnd) =>
        new(this, ComExceptionMapper.Invoke(() => Automation.ElementFromHandle(hwnd)), locator: null);

    /// <summary>The element under a screen point (physical pixels).</summary>
    public Element FromPoint(ScreenPoint point) =>
        new(this, PhysicalDpi.Run(() => ComExceptionMapper.Invoke(() => Automation.ElementFromPoint(new tagPOINT { x = point.X, y = point.Y }))), locator: null);

    /// <summary>The element that has keyboard focus.</summary>
    public Element GetFocusedElement() =>
        new(this, ComExceptionMapper.Invoke(() => Automation.GetFocusedElement()), locator: null);

    /// <summary>
    /// Calls <paramref name="probe"/> every <see cref="PollingInterval"/> until it returns non-null, and returns that
    /// result; returns null when <paramref name="timeout"/> (<see cref="DefaultTimeout"/> unless given) elapses first.
    /// The probe runs at least once, so a zero timeout means "look now".
    /// </summary>
    public T? WaitFor<T>(Func<T?> probe, TimeSpan? timeout = null) where T : class
    {
        var waited = Stopwatch.StartNew();
        var limit = timeout ?? DefaultTimeout;
        while (true)
        {
            if (probe() is { } result)
                return result;

            var remaining = limit - waited.Elapsed;
            if (remaining <= TimeSpan.Zero)
                return null;

            Thread.Sleep(remaining < PollingInterval ? remaining : PollingInterval);
        }
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> when called on an STA thread.</summary>
    internal static void EnsureMta()
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            throw new InvalidOperationException(
                "UI Automation must be used from an MTA thread. Run the automation code on a thread-pool thread " +
                "(for example with Task.Run) or on a thread created with ApartmentState.MTA.");
        }
    }

    /// <summary>The UIA condition object for a canonical search predicate (<see cref="ElementPredicate"/>).</summary>
    internal IUIAutomationCondition CreateCondition(Expression<Func<Element, bool>> search)
    {
        EnsureMta();
        var automation = Automation;
        return ComExceptionMapper.Invoke(() => ElementPredicate.ToUia(search, automation));
    }

    /// <summary>Releases the UIA client object.</summary>
    public void Dispose()
    {
        var automation = Interlocked.Exchange(ref _automation, null);
        if (automation is not null)
            Marshal.FinalReleaseComObject(automation);
    }
}
