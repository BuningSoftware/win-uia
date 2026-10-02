using WinUia.Core.Interop;

namespace WinUia.Core.Exceptions;

/// <summary>The element has no point that can be clicked (<c>UIA_E_NOCLICKABLEPOINT</c>).</summary>
public class UiaNoClickablePointException(string message, int hresult = HResults.UIA_E_NOCLICKABLEPOINT, Exception? innerException = null)
    : UiaException(message, hresult, innerException);
