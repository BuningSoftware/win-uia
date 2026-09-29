using System.Runtime.InteropServices;

namespace WinUia.Interop;

/// <summary>ACTIVATEOPTIONS from shobjidl_core.h.</summary>
[Flags]
internal enum ActivateOptions
{
    None = 0x0,
    DesignMode = 0x1,
    NoErrorUI = 0x2,
    NoSplashScreen = 0x4,
}

[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C"), ClassInterface(ClassInterfaceType.None)]
internal class ApplicationActivationManager;

[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IApplicationActivationManager
{
    void ActivateApplication(
        [MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)] string? arguments,
        ActivateOptions options,
        out uint processId);
}
