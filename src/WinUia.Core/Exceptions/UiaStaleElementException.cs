namespace WinUia.Core.Exceptions;

/// <summary>The element is no longer available, typically because its UI was re-rendered or closed (<c>UIA_E_ELEMENTNOTAVAILABLE</c>).</summary>
public class UiaStaleElementException(string message, int hresult = unchecked((int)0x80040201), Exception? innerException = null)
    : UiaException(message, hresult, innerException);
