using WinUia.Testing.Shared;

namespace WinUia.UiTests;

/// <summary>A page object that knows its own executable and defaults, so it launches with <c>App.Launch&lt;LaunchableApp&gt;()</c>.</summary>
public sealed class LaunchableApp : App
{
    protected override string ExecutablePath => AppPaths.TestApp;

    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true, MainWindowTimeout = TimeSpan.FromSeconds(7) };

    public string Title => MainWindow.Name;
}
