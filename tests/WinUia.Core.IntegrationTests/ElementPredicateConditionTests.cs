namespace WinUia.Core.IntegrationTests;

/// <summary>Canonical search predicates turned into real UIA condition objects (the expression side is unit-tested).</summary>
public class ElementPredicateConditionTests
{
    [Test]
    public void Every_supported_form_becomes_a_UIA_condition()
    {
        using var context = new AutomationContext();
        var search = ElementPredicate.Canonicalize(e =>
            (e.AutomationId == "a" && e.ProcessId == 1) || e.ClassName != "c" || e.ControlType == ControlType.Pane || !(e.Name == "n"));

        Assert.That(context.CreateCondition(search), Is.Not.Null);
    }
}
