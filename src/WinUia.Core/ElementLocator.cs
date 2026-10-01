using System.Linq.Expressions;
using WinUia.Core.Interop;

namespace WinUia.Core;

/// <summary>
/// Remembers how an element was found so it can be found again when UIA reports the original COM object as stale.
/// </summary>
/// <param name="Parent">The element searched from; null for the root.</param>
/// <param name="Search">The canonical search predicate (<see cref="ElementPredicate"/>).</param>
/// <param name="Scope">The search scope.</param>
/// <param name="Index">
/// The position among all matches for <c>FindAll</c> results; null for <c>FindFirst</c> results. That position
/// can change, so re-resolving an indexed locator checks the candidate is the same control
/// (<see cref="ElementIdentity"/>) rather than trusting the index.
/// </param>
internal sealed record ElementLocator(Element? Parent, Expression<Func<Element, bool>> Search, TreeScope Scope, int? Index)
{
    /// <summary>Locator for the desktop root element.</summary>
    public static readonly ElementLocator Root = new(null, ElementPredicate.MatchAll, TreeScope.Element, Index: null);

    /// <summary>
    /// Finds the element once, without waiting. Returns null when it does not exist right now, or — for
    /// indexed locators — when no match can be confirmed to be the same control as <paramref name="identity"/>.
    /// A stale parent re-resolves itself once, also without waiting; when that fails too, the stale exception propagates.
    /// </summary>
    public IUIAutomationElement? ResolveOnce(AutomationContext context, ElementIdentity? identity)
    {
        if (Parent is null)
            return ComExceptionMapper.Invoke(() => context.Automation.GetRootElement());

        var condition = context.CreateCondition(Search);
        if (Index is not { } index)
            return Parent.GetResolvingOnce(p => p.FindFirst(Scope, condition));

        if (identity is null)
            return null; // Never read before it went stale: nothing to confirm a candidate against.

        return Parent.GetResolvingOnce(p =>
        {
            var all = p.FindAll(Scope, condition);
            var length = all.Length;
            if (index < length && all.GetElement(index) is var atIndex && identity.Matches(atIndex))
                return atIndex;

            for (var i = 0; i < length; i++)
            {
                var candidate = all.GetElement(i);
                if (identity.Matches(candidate))
                    return candidate;
            }

            return null;
        });
    }

    public override string ToString() => Parent is null ? "<root>" : $"{Scope} {ElementPredicate.Describe(Search)}" + (Index is { } i ? $" [{i}]" : "");
}
