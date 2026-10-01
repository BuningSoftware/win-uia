namespace WinUia.Core.Exceptions;

/// <summary>The element is disabled and cannot be interacted with (<c>UIA_E_ELEMENTNOTENABLED</c>).</summary>
public class UiaElementNotEnabledException(string message, int hresult = HResults.UIA_E_ELEMENTNOTENABLED, Exception? innerException = null)
    : UiaException(message, hresult, innerException);
