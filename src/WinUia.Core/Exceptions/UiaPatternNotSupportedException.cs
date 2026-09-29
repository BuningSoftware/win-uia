namespace WinUia.Core.Exceptions;

/// <summary>The element does not support the requested control pattern (<c>UIA_E_NOTSUPPORTED</c>).</summary>
public class UiaPatternNotSupportedException(string message, int hresult = unchecked((int)0x80040204), Exception? innerException = null)
    : UiaException(message, hresult, innerException);
