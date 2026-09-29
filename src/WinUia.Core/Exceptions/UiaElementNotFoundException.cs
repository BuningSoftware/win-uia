namespace WinUia.Core.Exceptions;

/// <summary>No element matched the search within the timeout.</summary>
public class UiaElementNotFoundException(string message)
    : UiaException(message);
