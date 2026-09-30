using NUnit.Framework;
using WinUia.Core;

namespace WinUia.NUnit;

/// <summary>
/// Assertions for UI state that the application updates asynchronously. Import with
/// <c>using static WinUia.NUnit.UiAssertions;</c> to call <c>Eventually(...)</c> directly.
/// </summary>
public static class UiAssertions
{
    /// <summary>
    /// Fails the test unless <paramref name="condition"/> becomes true within WinUia's default timeout, polling at its
    /// default interval (<see cref="Poll"/>), the same waiting the library itself does.
    /// </summary>
    /// <param name="condition">The UI state to wait for, read again on every poll.</param>
    /// <param name="because">Why the condition should become true, shown when it does not.</param>
    public static void Eventually(Func<bool> condition, string because = "") =>
        Assert.That(Poll.Until(condition, Poll.DefaultTimeout, Poll.DefaultInterval), Is.True,
            $"The condition did not become true within {Poll.DefaultTimeout.TotalMilliseconds:0} ms. {because}".TrimEnd());
}
