using System.ComponentModel;
using System.Diagnostics;
using WinUia.Core;
using WinUia.Core.Exceptions;

namespace WinUia.Launchers;

/// <summary>Starts, attaches to and closes applications, and finds their main window. <see cref="App"/> is the public API over it.</summary>
internal static class AppLauncher
{
    /// <summary>
    /// Starts a classic executable (WinForms, WPF, unpackaged WinUI 3, Win32; x86 or x64) with the arguments, working
    /// directory and environment of <paramref name="options"/>.
    /// </summary>
    public static Process LaunchExe(string path, AppLaunchOptions? options = null)
    {
        var startInfo = new ProcessStartInfo(path, options?.Arguments ?? "") { UseShellExecute = false };
        if (options?.WorkingDirectory is { } workingDirectory)
            startInfo.WorkingDirectory = workingDirectory;
        foreach (var (name, value) in options?.Environment ?? new Dictionary<string, string?>())
        {
            if (value is null)
                startInfo.Environment.Remove(name);
            else
                startInfo.Environment[name] = value;
        }

        try
        {
            return Process.Start(startInfo) ?? throw new AppProcessException($"Could not start '{path}'.");
        }
        catch (Win32Exception ex)
        {
            throw new AppProcessException($"Could not start '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Starts a packaged app by its AppUserModelID, for example <c>"Microsoft.WindowsCalculator_8wekyb3d8bbwe!App"</c>,
    /// with the arguments of <paramref name="options"/>. Windows decides a packaged app's working directory and
    /// environment, so asking for either throws <see cref="ArgumentException"/>.
    /// </summary>
    public static Process LaunchPackaged(string appUserModelId, AppLaunchOptions? options = null)
    {
        EnsurePackagedOptions(options);
        return Attach(PackagedAppActivator.Activate(appUserModelId, options?.Arguments));
    }

    /// <summary>Throws <see cref="ArgumentException"/> for options a packaged app cannot be started with.</summary>
    public static void EnsurePackagedOptions(AppLaunchOptions? options)
    {
        if (options?.WorkingDirectory is not null || options?.Environment is not null)
            throw new ArgumentException(
                "A packaged app starts with the working directory and environment Windows gives it; " +
                "WorkingDirectory and Environment apply to executables only.", nameof(options));
    }

    /// <summary>Attaches to a running process by id.</summary>
    public static Process Attach(int processId)
    {
        try
        {
            return Process.GetProcessById(processId);
        }
        catch (ArgumentException ex)
        {
            throw new AppProcessException($"No process with id {processId} is running.", ex);
        }
    }

    /// <summary>Attaches to the first running process with the given name (without ".exe").</summary>
    public static Process Attach(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0)
            throw new AppProcessException($"No process named '{processName}' is running.");

        foreach (var other in processes.Skip(1))
            other.Dispose();
        return processes[0];
    }

    /// <summary>
    /// Waits for the process's top-level window. Looks for a desktop child owned by the process, then for a
    /// window hosting it (<c>ApplicationFrameHost</c> for UWP apps), then for <see cref="Process.MainWindowHandle"/>.
    /// Throws <see cref="AppProcessException"/> when the process exits first, and <see cref="UiaElementNotFoundException"/>
    /// when the timeout (<see cref="AutomationContext.DefaultTimeout"/> unless given) elapses.
    /// </summary>
    public static Element GetMainWindow(AutomationContext context, Process process, TimeSpan? timeout = null)
    {
        var effectiveTimeout = timeout ?? context.DefaultTimeout;
        var root = context.GetRootElement();
        var processId = process.Id;

        var window = context.WaitFor(() =>
        {
            if (process.HasExited)
                throw new AppProcessException($"Process {processId} exited with code {process.ExitCode} before its main window appeared.");

            var own = root.TryFind(e => e.ProcessId == processId && e.ControlType == ControlType.Window, TreeScope.Children, TimeSpan.Zero);
            if (own is not null)
                return own;

            foreach (var frame in root.FindAll(e => e.ClassName == "ApplicationFrameWindow", TreeScope.Children))
            {
                if (frame.TryFind(e => e.ProcessId == processId, TreeScope.Children, TimeSpan.Zero) is not null)
                    return frame;
            }

            process.Refresh();
            return process.MainWindowHandle != 0 ? context.FromHandle(process.MainWindowHandle) : null;
        }, effectiveTimeout);

        return window ?? throw new UiaElementNotFoundException(
            $"The main window of process {process.Id} did not appear within {effectiveTimeout.TotalMilliseconds:0} ms.");
    }

    /// <summary>
    /// Closes the application: asks its main window to close (Window pattern), waits up to
    /// <paramref name="timeout"/> for it to exit, then kills the process tree.
    /// </summary>
    public static void Close(AutomationContext context, Process process, TimeSpan? timeout = null)
    {
        if (process.HasExited)
            return;

        var effectiveTimeout = timeout ?? context.DefaultTimeout;
        try
        {
            GetMainWindow(context, process, TimeSpan.Zero).WindowPattern.Close();
        }
        catch (Exception ex) when (ex is UiaException or AppProcessException)
        {
            // No window, no Window pattern, or the process just exited: fall through to waiting and killing.
        }

        if (process.WaitForExit(effectiveTimeout))
            return;

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Exited between the wait and the kill.
        }

        process.WaitForExit(effectiveTimeout);
    }
}
