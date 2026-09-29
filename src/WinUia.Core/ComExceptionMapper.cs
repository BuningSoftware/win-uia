using System.Runtime.InteropServices;
using WinUia.Core.Exceptions;

namespace WinUia.Core;

/// <summary>
/// The boundary between UIA's COM errors and WinUia's exceptions: every raw UIA call runs through
/// <see cref="Invoke{T}"/>, which translates its HRESULTs into WinUia exceptions.
/// </summary>
internal static class ComExceptionMapper
{
    private const int UIA_E_ELEMENTNOTENABLED = unchecked((int)0x80040200);
    private const int UIA_E_ELEMENTNOTAVAILABLE = unchecked((int)0x80040201);
    private const int UIA_E_NOCLICKABLEPOINT = unchecked((int)0x80040202);
    private const int UIA_E_NOTSUPPORTED = unchecked((int)0x80040204);
    private const int UIA_E_TIMEOUT = unchecked((int)0x80131505);
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int RPC_E_DISCONNECTED = unchecked((int)0x80010108);
    private const int RPC_S_SERVER_UNAVAILABLE = unchecked((int)0x800706BA);
    private const int RPC_S_CALL_FAILED = unchecked((int)0x800706BE);

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
