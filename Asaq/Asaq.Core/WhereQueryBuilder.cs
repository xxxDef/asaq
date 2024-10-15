using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public static class WhereQueryBuilder
{
    public static Expression<Func<T,bool>>? BuildExpression<T, TFields>(this WhereQuery<TFields> where)
        where TFields : class
    {
        var param = Expression.Parameter(typeof(T), "x");
        var expressons = CreateExpressions<T, TFields>(param, where).SelectMany(s => s).ToArray();

        if (expressons.Length == 0)
            return null;

        var body = expressons.Aggregate(Expression.AndAlso);
        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return lambda;
    }

    private static IEnumerable<IEnumerable<Expression>> CreateExpressions<T, TFields>(
        ParameterExpression param, WhereQuery<TFields> where) where TFields : class
    {
        var itemType = typeof(T);

        // match
        yield return GetFields(itemType, where.Match, t => t == typeof(string))
           .Select(f => ExpStringEquals(param, f.prop, (string)f.value));
        yield return GetFields(itemType, where.Match, t => t.IsValueType)
           .Select(f => ExpEqual(param, f.prop, f.value));

        // from
        yield return GetFields(itemType, where.From, t => t == typeof(string))
           .Select(f => ExpStringStartWith(param, f.prop, (string)f.value));
        yield return GetFields(itemType, where.From, t => t.IsValueType && t.HasGreaterThanOrEqual())
           .Select(f => ExpGreaterThanOrEqual(param, f.prop, f.value));

        // to

        yield return GetFields(itemType, where.To, t => t == typeof(string))
            .Select(f => ExpStringEndWith(param, f.prop, (string)f.value));
        yield return GetFields(itemType, where.To, t => t.IsValueType && t.HasLessThanOrEqual())
            .Select(f => ExpLessThanOrEqual(param, f.prop, f.value));

        // contains

        yield return GetFields(itemType, where.Contains, t => t == typeof(string))
            .Select(f => ExpStringContains(param, f.prop, (string)f.value));
    }

    private static Func<Type, bool> IsEnumComparedByStringName = type =>
    {
        //if (type.IsEnum)
        //{
        //    var jsonConverterAttr = (JsonConverterAttribute?)Attribute.GetCustomAttribute(type, typeof(JsonConverterAttribute));
        //    if (jsonConverterAttr != null && jsonConverterAttr.ConverterType == typeof(StringEnumConverter))
        //    {
        //        return true;
        //    }
        //}

        return false;
    };

    private static IEnumerable<(PropertyInfo prop, object value)> GetFields<TFields>(
        Type entityType, 
        TFields? fields, 
        Func<Type, bool> selector) where TFields : class
    {
        if (fields == null)
            yield break;

        foreach (var fieldProperty in typeof(TFields).GetProperties())
        {
            if (!selector(fieldProperty.PropertyType))
                continue;
            
            var prop = GetEntityProperty(entityType, fieldProperty);

            var value = fieldProperty.GetValue(fields);
            if (value == null)
                continue;

            yield return (prop, value);
        }
    }

    static Expression ExpEqual(ParameterExpression param, PropertyInfo paramProp, object value)
    {
        ////////////////////////////////////////////////
        // param.paramProp == value
        ////////////////////////////////////////////////

        return Expression.Equal(
                    Expression.Property(param, paramProp),
                    Expression.Constant(value, paramProp.PropertyType));
    }

    static Expression ExpLessThanOrEqual(ParameterExpression param, PropertyInfo paramProp, object value)
    {
        ////////////////////////////////////////////////
        // param.paramProp <= value
        ////////////////////////////////////////////////

        return Expression.LessThanOrEqual(
                    Expression.Property(param, paramProp),
                    Expression.Constant(value, paramProp.PropertyType));
    }

    static Expression ExpGreaterThanOrEqual(ParameterExpression param, PropertyInfo paramProp, object value)
    {
        ////////////////////////////////////////////////
        // param.paramProp >= value
        ////////////////////////////////////////////////

        return Expression.GreaterThanOrEqual(
                    Expression.Property(param, paramProp),
                    Expression.Constant(value, paramProp.PropertyType));
    }

    static Expression ExpStringStartWith(ParameterExpression param, PropertyInfo paramProp, string value)
    {
        ////////////////////////////////////////////////
        // param.paramProp?.StartWith(value)
        ////////////////////////////////////////////////
        var method = typeof(string).GetMethod("StartsWith", new[]
        {
            typeof(string), 
            //typeof(StringComparison) // - commented for EF, to prevent " Translation of the 'string.Equals' overload with a 'StringComparison' parameter is not supported. "
        });
        InvalidOperation.IfNull(method);

        return ExpNullCoalescingCall(
                    Expression.Property(param, paramProp),
                    method,
                    Expression.Constant(false),
                    Expression.Constant(value, paramProp.PropertyType)
              // - commented for EF 
              //,Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison))
              );
    }

    static Expression ExpStringEndWith(ParameterExpression param, PropertyInfo paramProp, string value)
    {
        ////////////////////////////////////////////////
        // param.paramProp?.EndWith(value)
        ////////////////////////////////////////////////
        var method = typeof(string).GetMethod("EndsWith", new[]
        {
            typeof(string), 
            //typeof(StringComparison) // - commented for EF, to prevent " Translation of the 'string.Equals' overload with a 'StringComparison' parameter is not supported. "
        });
        InvalidOperation.IfNull(method);

        return ExpNullCoalescingCall(
                    Expression.Property(param, paramProp),
                    method,
                    Expression.Constant(false),
                    Expression.Constant(value, paramProp.PropertyType)
                  // - commented for EF 
                  //,Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison))
                  );
    }

    static Expression ExpStringEquals(ParameterExpression param, PropertyInfo paramProp, string value)
    {
        ////////////////////////////////////////////////
        // param.paramProp?.Equals(value)
        ////////////////////////////////////////////////
        var method = typeof(string).GetMethod("Equals", new[]
        {
            typeof(string), 
            //typeof(StringComparison) // - commented for EF, to prevent " Translation of the 'string.Equals' overload with a 'StringComparison' parameter is not supported. "

        });
        InvalidOperation.IfNull(method);

        bool isEnumComparedByStringName = IsEnumComparedByStringName(paramProp.PropertyType.GetUnderlineNonNullableType());

        Expression instance = isEnumComparedByStringName
            ? Expression.Convert(
                    Expression.Convert(Expression.Property(param, paramProp), typeof(object)),
                    typeof(string))
            : Expression.Property(param, paramProp);

        return ExpNullCoalescingCall(
                    instance,
                    method,
                    Expression.Constant(false),
                    Expression.Constant(value, typeof(string))
                 // - commented for EF 
                 //, Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison))
                 );
    }

    static Expression ExpStringContains(ParameterExpression param, PropertyInfo paramProp, string value)
    {
        ////////////////////////////////////////////////
        // param.paramProp?.Contains(value)
        ////////////////////////////////////////////////
        var method = typeof(string).GetMethod("Contains", new[]
        {
            typeof(string), 
            //typeof(StringComparison) // - commented for EF, to prevent " Translation of the 'string.Equals' overload with a 'StringComparison' parameter is not supported. "
        });
        InvalidOperation.IfNull(method);

        return ExpNullCoalescingCall(
                    Expression.Property(param, paramProp),
                    method,
                    Expression.Constant(false),
                    Expression.Constant(value, paramProp.PropertyType)
                  // - commented for EF 
                  //, Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison))
                  );
    }

    public static Expression ExpNullCoalescingCall(Expression instance, MethodInfo method, Expression returnWhenNull, params Expression[] arguments)
    {
        ////////////////////////////////////////////////
        // insatnce?.method(arguments) ?? returnWhenNull
        ////////////////////////////////////////////////

        return Expression.Condition(Expression.NotEqual(instance, Expression.Constant(null)),
            Expression.Call(instance, method, arguments),
            returnWhenNull);
    }

    private static PropertyInfo GetEntityProperty(Type entityType, PropertyInfo fieldProperty)
    {
        var prop = entityType.GetProperty(fieldProperty.Name);

        InvalidOperation.IfNull(prop,
            $"property {fieldProperty.Name} defined in 'where' class isn't exist in entity class {entityType}");

        var isSameType = prop.PropertyType.GetUnderlineNonNullableType() == fieldProperty.PropertyType.GetUnderlineNonNullableType()
            || IsEnumComparedByStringName(prop.PropertyType.GetUnderlineNonNullableType()) && fieldProperty.PropertyType.GetUnderlineNonNullableType() == typeof(string);

        InvalidOperation.IfFalse(isSameType,
            $"property {fieldProperty.Name} defined in 'where' class has type {prop.PropertyType} but same property in entity class {entityType} has type {prop.PropertyType}");

        return prop;
    }
}
