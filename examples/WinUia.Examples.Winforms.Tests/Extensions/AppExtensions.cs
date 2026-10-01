using System.Reflection;
using WinUia.Examples.Winforms.Tests.Application;

namespace WinUia.Examples.Winforms.Tests.Extensions;

/// <summary>Launching the example app, as a static extension on <see cref="App"/>: <c>App.LaunchApplication()</c>.</summary>
public static class AppExtensions
{
    // Stamped in by the .csproj: the example app's exe next to this project.
    private static readonly string ApplicationPath =
        typeof(AppExtensions).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "ApplicationPath").Value!;

    extension(App)
    {
        /// <summary>Launches the example app as its <see cref="MainForm"/>; dispose that to close it.</summary>
        public static MainForm LaunchApplication() => App.Launch(ApplicationPath).As<MainForm>();
    }
}
