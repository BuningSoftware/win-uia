using System.Linq.Expressions;
using System.Reflection;
using WinUia.Core.Interop;

namespace WinUia.Core;

/// <summary>
/// The one place that understands search predicates such as <c>e => e.ControlType == ControlType.Button &amp;&amp; e.Name == "OK"</c>.
/// Searches run inside UIA, so a predicate may only use <c>==</c> and <c>!=</c> between Name, AutomationId,
/// ClassName, ControlType or ProcessId and a value, combined with <c>&amp;&amp;</c>, <c>||</c> and <c>!</c>.
/// <para>
/// A predicate is first brought into canonical form (<see cref="Canonicalize"/>): the same shape with every value
/// read into a constant. Canonical predicates are what elements remember to re-find themselves, and what
/// <see cref="Describe"/> and <see cref="ToUia"/> work on.
/// </para>
/// </summary>
internal static class ElementPredicate
{
    private const string Supported =
        "Use ==, != on Name, AutomationId, ClassName, ControlType or ProcessId, combined with &&, || and !.";

    private static readonly ParameterExpression ElementParameter = Expression.Parameter(typeof(Element), "e");

    /// <summary>The canonical predicate that matches every element.</summary>
    public static Expression<Func<Element, bool>> MatchAll { get; } = FromBody(Expression.Constant(true));

    /// <summary>
    /// The canonical form of <paramref name="predicate"/>. Captured variables are read here, once. Throws
    /// <see cref="NotSupportedException"/> when the predicate cannot run inside UIA.
    /// </summary>
    public static Expression<Func<Element, bool>> Canonicalize(Expression<Func<Element, bool>> predicate) =>
        FromBody(Fold<Expression>(predicate.Body, () => Expression.Constant(true), Equality, Expression.AndAlso, Expression.OrElse, Expression.Not));

    /// <summary>A canonical predicate from a canonical <paramref name="body"/>.</summary>
    private static Expression<Func<Element, bool>> FromBody(Expression body) => Expression.Lambda<Func<Element, bool>>(body, ElementParameter);

    /// <summary>The canonical <c>e.<paramref name="property"/> == <paramref name="value"/></c>.</summary>
    private static Expression Equality(string property, object value)
    {
        Expression member = Expression.Property(ElementParameter, property);
        if (member.Type.IsEnum)
            member = Expression.Convert(member, typeof(int)); // As the compiler does: enums compare as their underlying type.
        return Expression.Equal(member, Expression.Constant(value, member.Type));
    }

    /// <summary>A readable form of a canonical predicate, for example <c>(ControlType=50000 AND Name=OK)</c>.</summary>
    public static string Describe(Expression<Func<Element, bool>> search) =>
        Fold<string>(search.Body, () => "True", (property, value) => $"{property}={value}",
            (left, right) => $"({left} AND {right})", (left, right) => $"({left} OR {right})", inner => $"NOT {inner}");

    /// <summary>The UIA condition object for a canonical predicate.</summary>
    public static IUIAutomationCondition ToUia(Expression<Func<Element, bool>> search, IUIAutomation automation) =>
        Fold<IUIAutomationCondition>(search.Body, automation.CreateTrueCondition,
            (property, value) => automation.CreatePropertyCondition(PropertyId(property), value),
            automation.CreateAndCondition, automation.CreateOrCondition, automation.CreateNotCondition);

    private static T Fold<T>(
        Expression body, Func<T> matchAll, Func<string, object, T> property,
        Func<T, T, T> allOf, Func<T, T, T> anyOf, Func<T, T> negate)
    {
        return Visit(body);

        T Visit(Expression node) => node switch
        {
            ConstantExpression { Value: true } => matchAll(),
            ConstantExpression { Value: false } => negate(matchAll()),
            BinaryExpression { NodeType: ExpressionType.AndAlso } both => allOf(Visit(both.Left), Visit(both.Right)),
            BinaryExpression { NodeType: ExpressionType.OrElse } either => anyOf(Visit(either.Left), Visit(either.Right)),
            UnaryExpression { NodeType: ExpressionType.Not } negation => negate(Visit(negation.Operand)),
            BinaryExpression { NodeType: ExpressionType.Equal } equal => Compare(equal, property),
            BinaryExpression { NodeType: ExpressionType.NotEqual } notEqual => negate(Compare(notEqual, property)),
            _ => throw new NotSupportedException($"'{node}' cannot be turned into a UI Automation condition. {Supported}"),
        };
    }

    /// <summary>Folds <c>e.Property == value</c> (either way round), with the value normalised to what UIA expects.</summary>
    private static T Compare<T>(BinaryExpression comparison, Func<string, object, T> property)
    {
        var (member, valueSide) = ElementProperty(comparison.Left) is { } left
            ? (left, comparison.Right)
            : (ElementProperty(comparison.Right), comparison.Left);
        if (member is null)
            throw new NotSupportedException($"'{comparison}' does not compare an element property. {Supported}");

        var value = Evaluate(valueSide)
            ?? throw new NotSupportedException($"'{comparison}' compares with null; element properties are never null.");

        var name = member.Member.Name;
        return name switch
        {
            nameof(Element.Name) or nameof(Element.AutomationId) or nameof(Element.ClassName) => property(name, (string)value),
            nameof(Element.ControlType) or nameof(Element.ProcessId) => property(name, Convert.ToInt32(value)),
            _ => throw new NotSupportedException($"Element.{name} cannot be searched on. {Supported}"),
        };
    }

    /// <summary>The <c>e.Property</c> access in <paramref name="side"/>, looking through the enum-to-int conversion.</summary>
    private static MemberExpression? ElementProperty(Expression side) =>
        StripConvert(side) is MemberExpression { Expression: ParameterExpression parameter } member && parameter.Type == typeof(Element)
            ? member
            : null;

    private static Expression StripConvert(Expression node) =>
        node is UnaryExpression { NodeType: ExpressionType.Convert } convert ? StripConvert(convert.Operand) : node;

    /// <summary>
    /// Drops only conversions that keep the value as it is: nullable wrapping, enum to its underlying type and boxing.
    /// User-defined, numeric and other conversions stay, so they run exactly as C# would run them.
    /// </summary>
    private static Expression StripLosslessConvert(Expression node) =>
        node is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } convert
        && (convert.Type == typeof(object) || Unwrap(convert.Type) == Unwrap(convert.Operand.Type))
            ? StripLosslessConvert(convert.Operand)
            : node;

    private static Type Unwrap(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsEnum ? Enum.GetUnderlyingType(type) : type;
    }

    private static object? Evaluate(Expression side)
    {
        switch (StripLosslessConvert(side))
        {
            case ConstantExpression constant:
                return constant.Value;
            case MemberExpression { Expression: ConstantExpression closure, Member: FieldInfo field }:
                return field.GetValue(closure.Value); // A captured local: no need to compile anything.
        }

        try
        {
            return Expression.Lambda<Func<object?>>(Expression.Convert(side, typeof(object))).Compile()();
        }
        catch (InvalidOperationException ex)
        {
            throw new NotSupportedException($"'{side}' must be a value that does not depend on the element. {Supported}", ex);
        }
    }

    private static int PropertyId(string property) => property switch
    {
        nameof(Element.Name) => PropertyIds.Name,
        nameof(Element.AutomationId) => PropertyIds.AutomationId,
        nameof(Element.ClassName) => PropertyIds.ClassName,
        nameof(Element.ControlType) => PropertyIds.ControlType,
        nameof(Element.ProcessId) => PropertyIds.ProcessId,
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, "Not a searchable element property."),
    };
}
