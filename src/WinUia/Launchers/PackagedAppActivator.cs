using System.Runtime.InteropServices;
using WinUia.Interop;

namespace WinUia.Launchers;

/// <summary>
/// Starts packaged (MSIX/UWP/WinUI 3) apps through <c>IApplicationActivationManager</c>, which returns the
/// app's real process id (starting <c>explorer.exe shell:AppsFolder\...</c> only yields Explorer's).
/// </summary>
internal static class PackagedAppActivator
{
    public static int Activate(string appUserModelId, string? arguments)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global (a COM coclass: the object behind it implements the interface)
        var manager = (IApplicationActivationManager)new ApplicationActivationManager();
        try
        {
            manager.ActivateApplication(appUserModelId, arguments, ActivateOptions.NoErrorUI, out var processId);
            return (int)processId;
        }
        catch (COMException ex)
        {
            throw new AppProcessException($"Could not activate packaged app '{appUserModelId}' (0x{ex.HResult:X8}): {ex.Message}", ex);
        }
        finally
        {
            Marshal.ReleaseComObject(manager);
        }
    }
}
