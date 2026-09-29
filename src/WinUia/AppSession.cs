using System.Diagnostics;
using WinUia.Core;
using WinUia.Launchers;

namespace WinUia;

/// <summary>
/// One connection to a running application: its process and the <see cref="AutomationContext"/> used to automate
/// it, released together. <see cref="Automation"/> instances hold a session; the one that opened it owns it, and
/// views created with <see cref="Automation.As{TView}"/> share it.
/// </summary>
internal sealed class AppSession : IDisposable
{
    private readonly bool _ownsProcess;

    private AppSession(Process process, AutomationContext context, bool ownsProcess)
    {
        Process = process;
        Context = context;
        _ownsProcess = ownsProcess;
    }

    public Process Process { get; }

    public AutomationContext Context { get; }

    /// <summary>
    /// Creates the context, then starts or attaches to the process. The context comes first so that an STA thread
    /// fails before anything is started that would have to be killed again.
    /// </summary>
    /// <param name="connect">Starts or attaches to the process.</param>
    /// <param name="ownsProcess">True when the process was started for this session, so disposing closes it.</param>
    public static AppSession Open(Func<Process> connect, bool ownsProcess)
    {
        var context = new AutomationContext();
        try
        {
            return new AppSession(connect(), context, ownsProcess);
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    /// <summary>Closes the application if this session started it, then releases the process and the context.</summary>
    public void Dispose()
    {
        try
        {
            if (_ownsProcess)
                AppLauncher.Close(Context, Process);
        }
        finally
        {
            Process.Dispose();
            Context.Dispose();
        }
    }
}
