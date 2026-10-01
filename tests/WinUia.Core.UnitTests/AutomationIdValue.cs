namespace WinUia.Core.UnitTests;

/// <summary>A typed id with an implicit conversion to string, the way page objects often name their controls.</summary>
internal sealed record AutomationIdValue(string Value)
{
    public static implicit operator string(AutomationIdValue id) => id.Value;
}
