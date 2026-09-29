namespace WinUia.Core.Exceptions;

/// <summary>The element is disabled and cannot be interacted with (<c>UIA_E_ELEMENTNOTENABLED</c>).</summary>
public class UiaElementNotEnabledException(string message, int hresult = unchecked((int)0x80040200), Exception? innerException = null)
    : UiaException(message, hresult, innerException);
