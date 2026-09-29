namespace WinUia.Core.Exceptions;

/// <summary>A UI Automation call or wait timed out (<c>UIA_E_TIMEOUT</c>).</summary>
public class UiaTimeoutException(string message, int hresult = unchecked((int)0x80131505), Exception? innerException = null)
    : UiaException(message, hresult, innerException);
