using System.Runtime.InteropServices;
using WinUia.Core.Exceptions;
using static WinUia.Core.Interop.HResults;

namespace WinUia.Core;

/// <summary>
/// The boundary between UIA's COM errors and WinUia's exceptions: every raw UIA call runs through
/// <see cref="Invoke{T}"/>, which translates its HRESULTs into WinUia exceptions.
/// </summary>
internal static class ComExceptionMapper
{
    /// <summary>
    /// Runs <paramref name="call"/>, translating its failures through <see cref="Map"/>. The runtime turns some
    /// HRESULTs into other exception types (UIA_E_TIMEOUT, E_NOTIMPL), so those are translated as well.
    /// </summary>
    public static T Invoke<T>(Func<T> call)
    {
        try
        {
            return call();
        }
        catch (Exception ex) when (ex is COMException or TimeoutException or NotImplementedException)
        {
            throw Map(ex);
        }
    }

    /// <summary>True for HRESULTs meaning the element (or its process) is gone.</summary>
    public static bool IsStale(int hresult) =>
        hresult is UIA_E_ELEMENTNOTAVAILABLE or RPC_E_DISCONNECTED or RPC_S_SERVER_UNAVAILABLE or RPC_S_CALL_FAILED;

    /// <summary>Maps a failed UIA call. Accepts any exception because the runtime turns some HRESULTs into
    /// other types (<see cref="TimeoutException"/> for UIA_E_TIMEOUT, <see cref="NotImplementedException"/> for E_NOTIMPL).</summary>
    public static UiaException Map(Exception ex)
    {
        var hr = ex.HResult;
        if (IsStale(hr))
            return new UiaStaleElementException("The element is no longer available.", hr, ex);

        return hr switch
        {
            UIA_E_ELEMENTNOTENABLED => new UiaElementNotEnabledException("The element is not enabled.", hr, ex),
            UIA_E_NOCLICKABLEPOINT => new UiaNoClickablePointException("The element has no clickable point.", hr, ex),
            UIA_E_NOTSUPPORTED => new UiaPatternNotSupportedException("The operation is not supported by the element.", hr, ex),
            UIA_E_TIMEOUT => new UiaTimeoutException("The UI Automation call timed out.", hr, ex),
            E_NOTIMPL => new UiaPatternNotSupportedException("The element's provider does not implement this operation.", hr, ex),
            _ => new UiaException($"UI Automation call failed (0x{hr:X8}): {ex.Message}", hr, ex),
        };
    }
}
