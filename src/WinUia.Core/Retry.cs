using WinUia.Core.Exceptions;

namespace WinUia.Core;

/// <summary>Retries UIA calls that fail because the element went stale.</summary>
internal static class Retry
{
    /// <summary>
    /// Runs <paramref name="action"/>; when it fails because the element is stale, calls <paramref name="refresh"/>
    /// and tries again, up to <paramref name="maxRetries"/> times. When <paramref name="refresh"/> returns false
    /// (nothing to re-resolve from), the stale exception propagates.
    /// </summary>
    /// <remarks>
    /// The refresh runs in the catch block, not in an exception filter: it waits and calls UIA, and an exception
    /// filter runs before inner <c>finally</c> blocks and swallows whatever it throws.
    /// </remarks>
    public static T OnStale<T>(Func<T> action, Func<bool> refresh, int maxRetries)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return ComExceptionMapper.Invoke(action);
            }
            catch (UiaStaleElementException) when (attempt < maxRetries)
            {
                if (!refresh())
                    throw;
            }
        }
    }
}
