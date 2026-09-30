using WinUia.Core.Interop;

namespace WinUia.Core.Patterns;

/// <summary>The Text pattern: documents and rich text controls.</summary>
public sealed class TextPatternWrapper : PatternWrapper
{
    internal TextPatternWrapper(Element element)
        : base(element, PatternIds.Text, PropertyIds.IsTextPatternAvailable, "Text") { }

    /// <summary>The document's text, optionally truncated to <paramref name="maxLength"/> characters (-1 for all).</summary>
    public string GetText(int maxLength = -1) =>
        Call<IUIAutomationTextPattern, string?>(p => p.DocumentRange.GetText(maxLength)) ?? "";
}
