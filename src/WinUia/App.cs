using System.Diagnostics;
using System.Linq.Expressions;
using WinUia.Core;
using WinUia.Core.Exceptions;
using WinUia.Launchers;

namespace WinUia;

/// <summary>
/// An application under automation: its process, its own <see cref="AutomationContext"/> and its windows.
/// <code>
/// using var app = App.Launch(@"C:\path\to\MyApp.exe");
/// app.Find("btnOk").Click();
/// </code>
/// A page object for the whole application derives from <see cref="App"/> and lists its controls; get one with
/// <see cref="As{TApp}"/>, for example <c>App.Launch(path).As&lt;MainPage&gt;()</c>. Page objects for part of the UI
/// are plain classes built on an <see cref="Element"/>, for example <c>new SettingsDialog(app.FindWindow("Settings"))</c>.
/// <para>
/// Must be used from an MTA thread (see <see cref="AutomationContext"/>); creating it on an STA thread throws.
/// Disposing an app that was launched closes it; disposing an attached app leaves it running.
/// </para>
/// </summary>
public class App : IDisposable
{
    private Process? _process;
    private AutomationContext? _context;
    private bool _ownsProcess;
    private bool _handedOver; // True once As<TApp> moved this app to another instance.
    private bool _disposed;

    /// <summary>
    /// For derived types, which are not created directly but through <see cref="As{TApp}"/>; that connects the instance
    /// to a running application.
    /// </summary>
    protected App() { }

    /// <summary>Starts a classic executable. Throws <see cref="AppProcessException"/> when it cannot be started.</summary>
    public static App Launch(string path, string? arguments = null) =>
        Open(() => AppLauncher.LaunchExe(path, arguments), ownsProcess: true);

    /// <summary>Starts a packaged app by its AppUserModelID. Throws <see cref="AppProcessException"/> when it cannot be activated.</summary>
    public static App LaunchPackaged(string appUserModelId, string? arguments = null) =>
        Open(() => AppLauncher.LaunchPackaged(appUserModelId, arguments), ownsProcess: true);

    /// <summary>Attaches to a running process by id. Throws <see cref="AppProcessException"/> when it is not running.</summary>
    public static App Attach(int processId) =>
        Open(() => AppLauncher.Attach(processId), ownsProcess: false);

    /// <summary>Attaches to a running process by name (without ".exe"). Throws <see cref="AppProcessException"/> when none is running.</summary>
    public static App Attach(string processName) =>
        Open(() => AppLauncher.Attach(processName), ownsProcess: false);

    /// <summary>The automation context used for this app.</summary>
    public AutomationContext Context => _context ?? throw NotConnected();

    /// <summary>The application's process.</summary>
    public Process Process => _process ?? throw NotConnected();

    /// <summary>How long <see cref="MainWindow"/> waits for the window to appear the first time. Default 20 seconds.</summary>
    public TimeSpan MainWindowTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>The application's main window, waited for on first use.</summary>
    public Element MainWindow => field ??= AppLauncher.GetMainWindow(Context, Process, MainWindowTimeout);

    /// <summary>
    /// This running application as <typeparamref name="TApp"/>, a page object that derives from <see cref="App"/>:
    /// <c>App.Launch(path).As&lt;MainPage&gt;()</c>. The returned instance takes the application over: dispose that one.
    /// Disposing this instance afterwards does nothing, so a launched application is closed exactly once.
    /// </summary>
    public TApp As<TApp>() where TApp : App, new()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_handedOver)
            throw new InvalidOperationException("This app was already handed over by As<TApp>(); use the instance it returned.");

        var app = new TApp();
        App connected = app; // Private members are reachable through the base type only.
        connected._process = Process;
        connected._context = Context;
        connected._ownsProcess = _ownsProcess;
        connected.MainWindowTimeout = MainWindowTimeout;
        _handedOver = true;
        return app;
    }

    /// <summary>
    /// Waits for a descendant of the main window whose AutomationId, or failing that Name, equals
    /// <paramref name="automationIdOrName"/> (<see cref="Element.FindByAutomationIdOrName"/>).
    /// Throws <see cref="UiaElementNotFoundException"/> on timeout.
    /// </summary>
    public Element Find(string automationIdOrName, TimeSpan? timeout = null) =>
        MainWindow.FindByAutomationIdOrName(automationIdOrName, timeout);

    /// <summary>Like <see cref="Find"/>, but returns null on timeout.</summary>
    public Element? TryFind(string automationIdOrName, TimeSpan? timeout = null) =>
        MainWindow.TryFindByAutomationIdOrName(automationIdOrName, timeout);

    /// <summary>
    /// Waits for a window of this application titled <paramref name="title"/>, such as a dialog. Owned windows are
    /// listed under their owner or under the desktop depending on the UI framework, so both are searched.
    /// Throws <see cref="UiaElementNotFoundException"/> on timeout.
    /// </summary>
    public Element FindWindow(string title, TimeSpan? timeout = null)
    {
        var effectiveTimeout = timeout ?? Context.DefaultTimeout;
        return TryFindWindow(title, effectiveTimeout)
            ?? throw new UiaElementNotFoundException(
                $"No window titled '{title}' of process {Process.Id} appeared within {effectiveTimeout.TotalMilliseconds:0} ms.");
    }

    /// <summary>Like <see cref="FindWindow"/>, but returns null on timeout.</summary>
    public Element? TryFindWindow(string title, TimeSpan? timeout = null)
    {
        var processId = Process.Id;
        Expression<Func<Element, bool>> isWindow = e => e.Name == title && e.ControlType == ControlType.Window && e.ProcessId == processId;
        var owner = MainWindow;
        var desktop = Context.GetRootElement();
        return Context.WaitFor(
            () => owner.TryFind(isWindow, TreeScope.Children, TimeSpan.Zero)
                  ?? desktop.TryFind(isWindow, TreeScope.Children, TimeSpan.Zero),
            timeout);
    }

    /// <summary>Closes the application: Window pattern first, then kill after <paramref name="timeout"/>.</summary>
    public void Close(TimeSpan? timeout = null) => AppLauncher.Close(Context, Process, timeout);

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

        // Not connected (created directly), or handed over to the instance As<TApp> returned: nothing to release here.
        if (_handedOver || _process is null || _context is null)
            return;

        try
        {
            if (_ownsProcess)
                Close();
        }
        finally
        {
            _process.Dispose();
            _context.Dispose();
        }
    }

    /// <summary>
    /// Creates the context, then starts or attaches to the process. The context comes first so that an STA thread
    /// fails before anything is started that would have to be killed again.
    /// </summary>
    private static App Open(Func<Process> connect, bool ownsProcess)
    {
        var context = new AutomationContext();
        try
        {
            return new App { _process = connect(), _context = context, _ownsProcess = ownsProcess };
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    private static InvalidOperationException NotConnected() =>
        new("This app is not connected to an application. Get it with App.Launch(...).As<TApp>() or App.Attach(...).As<TApp>().");
}
