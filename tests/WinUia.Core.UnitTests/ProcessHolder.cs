using System.Linq.Expressions;

namespace WinUia.Core.UnitTests;

/// <summary>Stands in for a page object whose search reads one of its own members.</summary>
internal sealed class ProcessHolder(int id)
{
    public int Id { get; } = id;

    public Expression<Func<Element, bool>> Predicate() => e => e.ProcessId == Id;
}
