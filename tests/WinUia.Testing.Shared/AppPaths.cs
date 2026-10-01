using System.Reflection;

namespace WinUia.Testing.Shared;

/// <summary>
/// Locations of the apps under tests/ that the library UI tests launch, stamped into this assembly by
/// WinUia.Testing.Shared.csproj (one <c>AppPath.&lt;Name&gt;</c> metadata attribute per app). Add a property here for
/// each new app.
/// </summary>
public static class AppPaths
{
    /// <summary>The test app, tests/WinUia.TestApp: an empty window.</summary>
    public static string TestApp { get; } = Get("TestApp");

    private static string Get(string name) =>
        typeof(AppPaths).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == $"AppPath.{name}").Value!;
}
