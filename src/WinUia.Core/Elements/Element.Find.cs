using System.Linq.Expressions;
using WinUia.Core.Exceptions;

namespace WinUia.Core.Elements;

public sealed partial class Element
{
    /// <summary>
    /// Waits for the first element matching <paramref name="predicate"/>, for example
    /// <c>Find(e => e.ControlType == ControlType.Button &amp;&amp; e.Name == "OK")</c>, and returns it.
    /// The predicate runs inside UIA, so it may only compare Name, AutomationId, ClassName, ControlType or ProcessId
    /// with <c>==</c> and <c>!=</c>, combined with <c>&amp;&amp;</c>, <c>||</c> and <c>!</c>; anything else throws
    /// <see cref="NotSupportedException"/>. Throws <see cref="UiaElementNotFoundException"/> when no element appears
    /// within the timeout (<see cref="AutomationContext.DefaultTimeout"/> unless given).
    /// </summary>
    public Element Find(Expression<Func<Element, bool>> predicate, TreeScope scope = TreeScope.Descendants, TimeSpan? timeout = null)
    {
        var search = ElementPredicate.Canonicalize(predicate);
        var effectiveTimeout = timeout ?? Context.DefaultTimeout;
        return TryFind(search, scope, effectiveTimeout)
            ?? throw new UiaElementNotFoundException(
                $"No element matching {ElementPredicate.Describe(search)} was found in scope {scope} of {this} within {effectiveTimeout.TotalMilliseconds:0} ms.");
    }

    /// <summary>Like <see cref="Find(Expression{Func{Element, bool}}, TreeScope, TimeSpan?)"/>, but returns null on timeout.</summary>
    public Element? TryFind(Expression<Func<Element, bool>> predicate, TreeScope scope = TreeScope.Descendants, TimeSpan? timeout = null)
    {
        var search = ElementPredicate.Canonicalize(predicate);
        return Context.WaitFor(() => FindFirstNow(search, scope), timeout);
    }

    /// <summary>All elements matching <paramref name="predicate"/> right now (no waiting).</summary>
    public IReadOnlyList<Element> FindAll(Expression<Func<Element, bool>> predicate, TreeScope scope = TreeScope.Descendants)
    {
        var search = ElementPredicate.Canonicalize(predicate);
        var uiaCondition = Context.CreateCondition(search);
        var array = Get(e => e.FindAll(scope, uiaCondition));
        return ComExceptionMapper.Invoke(() =>
        {
            var length = array.Length;
            var result = new List<Element>(length);
            for (var i = 0; i < length; i++)
                result.Add(new Element(Context, array.GetElement(i), new ElementLocator(this, search, scope, i)));
            return result;
        });
    }

    /// <summary>
    /// Waits for a descendant with the given AutomationId, the stable identifier page objects look controls up by.
    /// For anything else, pass a lambda to <see cref="Find(Expression{Func{Element, bool}}, TreeScope, TimeSpan?)"/>.
    /// </summary>
    public Element FindByAutomationId(string automationId, TimeSpan? timeout = null) =>
        Find(e => e.AutomationId == automationId, timeout: timeout);

    /// <summary>Waits for a descendant with the given AutomationId; returns null on timeout.</summary>
    public Element? TryFindByAutomationId(string automationId, TimeSpan? timeout = null) =>
        TryFind(e => e.AutomationId == automationId, timeout: timeout);

    /// <summary>The parent in the control view, or null for the root.</summary>
    public Element? Parent
    {
        get
        {
            var parent = Get(e => Context.Automation.ControlViewWalker.GetParentElement(e));
            return parent is null ? null : new Element(Context, parent, locator: null);
        }
    }

    /// <summary>The direct children in the control view.</summary>
    public IReadOnlyList<Element> Children => FindAll(e => true, TreeScope.Children);

    /// <summary>Looks once, without waiting. <paramref name="search"/> must be canonical (<see cref="ElementPredicate"/>).</summary>
    internal Element? FindFirstNow(Expression<Func<Element, bool>> search, TreeScope scope)
    {
        var uiaCondition = Context.CreateCondition(search);
        var found = Get(e => e.FindFirst(scope, uiaCondition));
        return found is null ? null : new Element(Context, found, new ElementLocator(this, search, scope, Index: null));
    }
}
