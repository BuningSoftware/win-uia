using System.Diagnostics;
using WinUia.Core;
using WinUia.Core.Elements;
using WinUia.Core.Exceptions;
using WinUia.Launchers;

namespace WinUia;

/// <summary>
/// An application under automation: its process, its own <see cref="AutomationContext"/> and its main window.
/// <code>
/// using var app = Automation.Launch(@"C:\path\to\MyApp.exe");
/// app.Find("btnOk").Click();
/// </code>
/// Derive from <see cref="Automation"/> to describe a specific application (its controls as properties) and create it
/// with the generic factories, for example <c>Automation.Launch&lt;MyAppWindow&gt;(path)</c>.
/// <para>
/// Must be used from an MTA thread (see <see cref="AutomationContext"/>); creating it on an STA thread throws.
/// Disposing an app that was launched closes it; disposing an attached app leaves it running.
/// </para>
/// </summary>
public class Automation : IDisposable
{
    private AppSession? _session;
    private bool _ownsSession; // False for views (As<TView>), which share the session of the app they came from.
    private bool _disposed;

    /// <summary>
    /// For derived types, which are created through <see cref="Launch{TApp}"/>, <see cref="LaunchPackaged{TApp}"/> or
    /// <see cref="Attach{TApp}(int)"/>; those factories connect the instance to its process.
    /// </summary>
    protected Automation() { }

    /// <summary>Starts a classic executable.</summary>
    public static Automation Launch(string path, string? arguments = null) =>
        Open(new Automation(), () => AppLauncher.LaunchExe(path, arguments), ownsProcess: true);

    /// <summary>Starts a classic executable as a <typeparamref name="TApp"/>.</summary>
    public static TApp Launch<TApp>(string path, string? arguments = null) where TApp : Automation, new() =>
        Open(new TApp(), () => AppLauncher.LaunchExe(path, arguments), ownsProcess: true);

    /// <summary>Starts a packaged app by its AppUserModelID.</summary>
    public static Automation LaunchPackaged(string appUserModelId, string? arguments = null) =>
        Open(new Automation(), () => AppLauncher.LaunchPackaged(appUserModelId, arguments), ownsProcess: true);

    /// <summary>Starts a packaged app by its AppUserModelID as a <typeparamref name="TApp"/>.</summary>
    public static TApp LaunchPackaged<TApp>(string appUserModelId, string? arguments = null) where TApp : Automation, new() =>
        Open(new TApp(), () => AppLauncher.LaunchPackaged(appUserModelId, arguments), ownsProcess: true);

    /// <summary>Attaches to a running process by id.</summary>
    public static Automation Attach(int processId) =>
        Open(new Automation(), () => AppLauncher.Attach(processId), ownsProcess: false);

    /// <summary>Attaches to a running process by id as a <typeparamref name="TApp"/>.</summary>
    public static TApp Attach<TApp>(int processId) where TApp : Automation, new() =>
        Open(new TApp(), () => AppLauncher.Attach(processId), ownsProcess: false);

    /// <summary>Attaches to a running process by name (without ".exe").</summary>
    public static Automation Attach(string processName) =>
        Open(new Automation(), () => AppLauncher.Attach(processName), ownsProcess: false);

    /// <summary>Attaches to a running process by name (without ".exe") as a <typeparamref name="TApp"/>.</summary>
    public static TApp Attach<TApp>(string processName) where TApp : Automation, new() =>
        Open(new TApp(), () => AppLauncher.Attach(processName), ownsProcess: false);

    /// <summary>The automation context used for this app.</summary>
    public AutomationContext Context => Session.Context;

    /// <summary>The application's process.</summary>
    public Process Process => Session.Process;

    private AppSession Session => _session ?? throw NotConnected();

    /// <summary>How long <see cref="MainWindow"/> waits for the window to appear the first time. Default 20 seconds.</summary>
    public TimeSpan MainWindowTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>The application's main window, waited for on first use.</summary>
    public Element MainWindow => field ??= AppLauncher.GetMainWindow(Context, Process, MainWindowTimeout);

    /// <summary>
    /// Waits for a descendant of the main window whose AutomationId, or failing that Name, equals
    /// <paramref name="automationIdOrName"/>. Throws <see cref="UiaElementNotFoundException"/> on timeout.
    /// </summary>
    public Element Find(string automationIdOrName, TimeSpan? timeout = null)
    {
        var effectiveTimeout = timeout ?? Context.DefaultTimeout;
        return TryFind(automationIdOrName, effectiveTimeout)
            ?? throw new UiaElementNotFoundException(
                $"No element with AutomationId or Name '{automationIdOrName}' was found in {MainWindow} within {effectiveTimeout.TotalMilliseconds:0} ms.");
    }

    /// <summary>Like <see cref="Find"/>, but returns null on timeout.</summary>
    public Element? TryFind(string automationIdOrName, TimeSpan? timeout = null)
    {
        var window = MainWindow;
        return Context.WaitFor(
            () => window.TryFind(e => e.AutomationId == automationIdOrName, TreeScope.Descendants, TimeSpan.Zero)
                ?? window.TryFind(e => e.Name == automationIdOrName, TreeScope.Descendants, TimeSpan.Zero),
            timeout);
    }

    /// <summary>Closes the application: Window pattern first, then kill after <paramref name="timeout"/>.</summary>
    public void Close(TimeSpan? timeout = null) => AppLauncher.Close(Context, Process, timeout);

    /// <summary>
    /// The same running application seen as <typeparamref name="TView"/>, typically a page object for one part of
    /// the UI (for example the page behind a selected tab). The view shares this app's process and context; it is
    /// not a new launch, and disposing it does nothing — dispose the app it came from.
    /// </summary>
    public TView As<TView>() where TView : Automation, new()
    {
        var view = new TView();
        Automation shared = view; // Private members are reachable through the base type only.
        shared._session = Session;
        shared.MainWindowTimeout = MainWindowTimeout;
        return view;
    }

    /// <summary>Closes the app if it was launched by this instance, then releases the process and context.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the app. Derived types override this to release their own resources, then call the base.</summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;
        _disposed = true;

        if (_ownsSession)
            _session?.Dispose();
    }

    private static TApp Open<TApp>(TApp app, Func<Process> connect, bool ownsProcess) where TApp : Automation
    {
        app._session = AppSession.Open(connect, ownsProcess);
        app._ownsSession = true;
        return app;
    }

    private static InvalidOperationException NotConnected() =>
        new("This app is not connected to a process. Create it with Automation.Launch, Automation.LaunchPackaged or Automation.Attach.");
}
