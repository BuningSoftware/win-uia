using System.Reflection;

namespace WinUia.Testing.Shared;

/// <summary>Location of the fixture app executable, stamped into this assembly by WinUia.Testing.Shared.csproj.</summary>
public static class TestAppPath
{
    public static string Exe { get; } =
        typeof(TestAppPath).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "TestAppPath").Value!;
}
