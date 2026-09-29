using System.Linq.Expressions;
using WinUia.Core.Exceptions;

namespace WinUia.Core.Elements;

/// <summary>Fluent helpers for navigating and querying the element tree.</summary>
public static class ElementExtensions
{
    /// <summary>Walks up the control view from the parent to the root.</summary>
    public static IEnumerable<Element> Ancestors(this Element element)
    {
        for (var current = element.Parent; current is not null; current = current.Parent)
            yield return current;
    }

    /// <summary>
    /// Follows a '/'-separated path of children, each step matched by AutomationId, then Name, then control
    /// type name (for example <c>"Pane/btnOk"</c> or <c>"Window/Edit"</c>). Each step waits up to the timeout.
    /// </summary>
    public static Element Path(this Element element, string path, TimeSpan? timeout = null)
    {
        var current = element;
        foreach (var step in path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            Expression<Func<Element, bool>> matchesStep = e => e.AutomationId == step || e.Name == step;
            if (Enum.TryParse<ControlType>(step, ignoreCase: false, out var controlType))
                matchesStep = e => e.AutomationId == step || e.Name == step || e.ControlType == controlType;

            current = current.TryFind(matchesStep, TreeScope.Children, timeout)
                ?? throw new UiaElementNotFoundException($"Path step '{step}' of '{path}' was not found under {current}.");
        }

        return current;
    }
}
