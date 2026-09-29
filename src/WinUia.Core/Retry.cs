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
    public static T OnStale<T>(Func<T> action, Func<bool> refresh, int maxRetries)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return ComExceptionMapper.Invoke(action);
            }
            catch (UiaStaleElementException) when (attempt < maxRetries && refresh())
            {
            }
        }
    }
}
