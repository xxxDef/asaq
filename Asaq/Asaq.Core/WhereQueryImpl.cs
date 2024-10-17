using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public abstract class WhereQueryImpl<TFields> : IWhereQuery
{
    protected abstract Expression CreateExpresssion(
        ParameterExpression input,
        string conditionName,
        PropertyInfo prop,
        object value);

    public IEnumerable<Expression> CreateExpressions(ParameterExpression input)
    {
        foreach (var clause in this.GetinitializedProperies<TFields?>())
        {
            var conditions = GetConditions(input.Type, clause.value);

            foreach (var (prop, values) in conditions)
            {
                var epressions = values
                    .Select(value => CreateExpresssion(input, clause.prop.Name, prop, value))
                    .ToArray();

                if (epressions.Length > 0)
                    yield return epressions.Aggregate(Expression.OrElse);
            }
        }
    }

    private static IEnumerable<(PropertyInfo prop, IEnumerable<object>)> GetConditions(Type entityType, object clauseObject) =>
        from clause in clauseObject.GetinitializedProperies()
        let values = GetExpandedValues(clause.prop, clauseObject)
        where values is not null
        let entityProp = GetEntityProperty(entityType, clause.prop)
        select (entityProp, values);

    private static IEnumerable<object> GetExpandedValues(PropertyInfo conditionProp, object conditionObject)
    {
        var value = conditionProp.GetValue(conditionObject);
        if (value == null)
            yield break;

        if (conditionProp.PropertyType.IsArray)
        {
            foreach (var o in (IEnumerable)value)
            {
                if (o is not null)
                    yield return o;
            }
        }
        else
        {
            yield return value;
        }
    }

    private static PropertyInfo GetEntityProperty(Type entityType, PropertyInfo fieldProperty)
    {
        var entityProp = entityType.GetProperty(fieldProperty.Name);

        InvalidOperation.IfNull(entityProp,
            $"property {fieldProperty.Name} defined in 'where' class isn't exist in entity class {entityType}");

        var underlineEntityProp = entityProp.PropertyType.GetUnderlineNonNullableType();

        var underlineFieldProp = fieldProperty.PropertyType.IsArray
            ? fieldProperty.PropertyType.GetElementType()?.GetUnderlineNonNullableType()
            : fieldProperty.PropertyType.GetUnderlineNonNullableType();

        InvalidOperation.IfNull($"Unknown element type of array {fieldProperty.PropertyType} in 'where' object");

        var isSameType = underlineEntityProp == underlineFieldProp
            || ExpressionHelper.IsEnumComparedByStringName(underlineEntityProp) && underlineEntityProp == typeof(string);

        InvalidOperation.IfFalse(isSameType,
            $"property {fieldProperty.Name} defined in 'where' class has type {entityProp.PropertyType} but same property in entity class {entityType} has type {entityProp.PropertyType}");

        return entityProp;
    }
}
