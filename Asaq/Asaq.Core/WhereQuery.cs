using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public class WhereQuery<TFields> : WhereQueryImpl<TFields>
{
    public TFields? Match { get; set; }
    public TFields? Contains { get; set; }
    public TFields? From { get; set; }
    public TFields? To { get; set; }

    protected override Expression CreateExpresssion(ParameterExpression input, string conditionName, PropertyInfo prop, object value)
    => conditionName switch
    {
        "Match" => input.GetExpression_Match(prop, value),
        "From" => input.GetExpression_From(prop, value),
        "To" => input.GetExpression_To(prop, value),
        "Contains" => input.GetExpression_Contains(prop, value),
        _ => throw new NotImplementedException($"Condition {conditionName} is not supported")
    };
}
