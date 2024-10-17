using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public static class ExpressionHelper
{
    public static Expression GetExpression_Match(this ParameterExpression input, PropertyInfo prop, object value)
        =>
        prop.PropertyType.IsString() ? input.ExpStringEquals(prop, (string)value)
        : prop.PropertyType.IsValueType ? input.ExpEqual(prop, value)
        : throw new NotSupportedException();

    public static Expression GetExpression_From(this ParameterExpression input, PropertyInfo prop, object value)
        =>
        prop.PropertyType.IsString() ? input.ExpStringEndWith(prop, (string)value)
        : prop.PropertyType.IsValueType ? input.ExpLessThanOrEqual(prop, value)
        : throw new NotSupportedException();

    public static Expression GetExpression_To(this ParameterExpression input, PropertyInfo prop, object value)
        =>
        prop.PropertyType.IsString() ? input.ExpStringStartWith(prop, (string)value)
        : prop.PropertyType.IsValueType ? input.ExpGreaterThanOrEqual(prop, value)
        : throw new NotSupportedException();

    public static Expression GetExpression_Contains(this ParameterExpression input, PropertyInfo prop, object value)
        =>
        prop.PropertyType.IsString() ? input.ExpStringContains(prop, (string)value)
         : throw new NotSupportedException();
    
    ////////////////////////////////////////////////
    // param.paramProp == value
    ////////////////////////////////////////////////
    public static Expression ExpEqual(this ParameterExpression param, PropertyInfo paramProp, object value)
    =>  Expression.Equal(
            Expression.Property(param, paramProp),
            Expression.Constant(value, paramProp.PropertyType));

    ////////////////////////////////////////////////
    // param.paramProp <= value
    ////////////////////////////////////////////////
    public static Expression ExpLessThanOrEqual(this ParameterExpression param, PropertyInfo paramProp, object value) 
    => Expression.LessThanOrEqual(
            Expression.Property(param, paramProp),
            Expression.Constant(value, paramProp.PropertyType));

    ////////////////////////////////////////////////
    // param.paramProp >= value
    ////////////////////////////////////////////////
    public static Expression ExpGreaterThanOrEqual(this ParameterExpression param, PropertyInfo paramProp, object value)
    => Expression.GreaterThanOrEqual(
            Expression.Property(param, paramProp),
            Expression.Constant(value, paramProp.PropertyType));

    public static Expression ExpStringStartWith(this ParameterExpression param, PropertyInfo paramProp, string value)
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


    public static Expression ExpStringEndWith(this ParameterExpression param, PropertyInfo paramProp, string value)
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

    public static Expression ExpStringEquals(this ParameterExpression param, PropertyInfo paramProp, string value)
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

    public static Expression ExpStringContains(this ParameterExpression param, PropertyInfo paramProp, string value)
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

    public static Func<Type, bool> IsEnumComparedByStringName = type =>
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
}