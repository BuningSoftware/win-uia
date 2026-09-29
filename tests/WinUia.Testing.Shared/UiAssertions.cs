using NUnit.Framework;

namespace WinUia.Testing.Shared;

/// <summary>
/// Assertions for UI state that the application updates asynchronously. Imported with <c>using static</c> into every
/// test project (tests/Directory.Build.props), so tests call <c>Eventually(...)</c> directly.
/// </summary>
public static class UiAssertions
{
    private const int TIMEOUT_MILLISECONDS = 5000;
    private const int POLLING_INTERVAL_MILLISECONDS = 100;

    /// <summary>Fails the test unless <paramref name="condition"/> becomes true within five seconds, polling in between.</summary>
    /// <param name="condition">The UI state to wait for, read again on every poll.</param>
    /// <param name="because">Why the condition should become true, shown when it does not.</param>
    public static void Eventually(Func<bool> condition, string because = "") =>
        Assert.That(condition, Is.True.After(TIMEOUT_MILLISECONDS, POLLING_INTERVAL_MILLISECONDS), because);
}
