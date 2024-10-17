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

    public IEnumerable<Expression> CreateExpressions<T>(ParameterExpression input)
    {
        var itemType = typeof(T);

        foreach (var parametersProps in GetType().GetProperties<TFields?>())
        {        
            var parameters = parametersProps.GetValue(this);
            if (parameters == null)
                continue;

            var conditions = GetConditions(itemType, parameters);

            foreach (var (prop, value) in conditions)
            {
                yield return CreateExpresssion(input, parametersProps.Name, prop, value);
            }
        }
    }

    private static IEnumerable<(PropertyInfo prop, object value)> GetConditions(Type entityType, object conditions)
        =>
        from conditionProp in conditions.GetType().GetProperties()
        let entityProp = GetEntityProperty(entityType, conditionProp)
        let value = conditionProp.GetValue(conditions)
        where value is not null
        select (entityProp, value);

    private static PropertyInfo GetEntityProperty(Type entityType, PropertyInfo fieldProperty)
    {
        var prop = entityType.GetProperty(fieldProperty.Name);

        InvalidOperation.IfNull(prop,
            $"property {fieldProperty.Name} defined in 'where' class isn't exist in entity class {entityType}");

        var isSameType = prop.PropertyType.GetUnderlineNonNullableType() == fieldProperty.PropertyType.GetUnderlineNonNullableType()
            || ExpressionHelper.IsEnumComparedByStringName(prop.PropertyType.GetUnderlineNonNullableType()) && fieldProperty.PropertyType.GetUnderlineNonNullableType() == typeof(string);

        InvalidOperation.IfFalse(isSameType,
            $"property {fieldProperty.Name} defined in 'where' class has type {prop.PropertyType} but same property in entity class {entityType} has type {prop.PropertyType}");

        return prop;
    }
}
