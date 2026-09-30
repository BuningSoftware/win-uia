using System.Linq.Expressions;
using WinUia.Core.Exceptions;

namespace WinUia.Core;

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
    /// type name (for example <c>"Pane/btnOk"</c> or <c>"Window/Edit"</c>); the first kind of match that finds a child
    /// wins. Each step waits up to the timeout.
    /// </summary>
    public static Element Path(this Element element, string path, TimeSpan? timeout = null)
    {
        var current = element;
        foreach (var step in path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            List<Expression<Func<Element, bool>>> searches = [.. Element.ByAutomationIdOrName(step)];
            if (Enum.TryParse<ControlType>(step, ignoreCase: false, out var controlType))
                searches.Add(ElementPredicate.Canonicalize(e => e.ControlType == controlType));

            current = current.TryFindFirstOf(searches, TreeScope.Children, timeout)
                ?? throw new UiaElementNotFoundException($"Path step '{step}' of '{path}' was not found under {current}.");
        }

        return current;
    }
}
