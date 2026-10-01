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
