namespace WinUia.Core.Exceptions;

/// <summary>The element has no point that can be clicked (<c>UIA_E_NOCLICKABLEPOINT</c>).</summary>
public class UiaNoClickablePointException(string message, int hresult = unchecked((int)0x80040202), Exception? innerException = null)
    : UiaException(message, hresult, innerException);
