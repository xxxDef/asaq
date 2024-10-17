using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public static class TypeInfoEx
{
    public static IEnumerable<PropertyInfo> GetProperties<T>(this Type type)
        => from p in type.GetProperties()
           where p.PropertyType == typeof(T)
           select p;
    
    public static bool HasLessThanOrEqual(this Type t)
    {
        var op = t.GetUnderlineNonNullableType().GetMethod("op_LessThanOrEqual");
        return op != null && op.IsSpecialName;
    }
    public static bool HasGreaterThanOrEqual(this Type t)
    {
        var op = t.GetUnderlineNonNullableType().GetMethod("op_GreaterThanOrEqual");
        return op != null && op.IsSpecialName;
    }

    public static Type GetUnderlineNonNullableType(this Type t) => Nullable.GetUnderlyingType(t) ?? t;

    public static bool IsEnumerable(this PropertyInfo p)
    {
        return p.PropertyType.IsEnumerable();
    }
    public static bool IsEnumerable(this Type t)
    {
        return typeof(IEnumerable).IsAssignableFrom(t);
    }
 
    public static bool IsString(this PropertyInfo prop)
    {
        return prop.PropertyType == typeof(string);
    }

    public static bool IsSimpleTypeOrString(this Type type)
    {
        return type.IsValueType || type.IsString();
    }
    public static bool IsString(this Type type)
    {
        return type == typeof(string);
    }

 
    static readonly ConcurrentDictionary<MemberInfo, Attribute[]> attributesInfoCache = new ConcurrentDictionary<MemberInfo, Attribute[]>();
    public static Attribute[] GetCustomAttributesEx(this MemberInfo element)
    {
        InvalidOperation.IfNull(element);

        if (attributesInfoCache.TryGetValue(element, out var res))
            return res;

        res = Attribute.GetCustomAttributes(element, true);
        attributesInfoCache.TryAdd(element, res);

        return res;
    }

    public static IEnumerable<TAttibute> GetCustomAttributesEx<TAttibute>(this MemberInfo element)
    {
        return element.GetCustomAttributesEx().OfType<TAttibute>().Cast<TAttibute>();
    }

    public static PropertyInfo GetPropertyInfo<T, P>(Expression<Func<T, P>> propertyExpr)
    {
        var expr = propertyExpr.Body is UnaryExpression uexpr ? (MemberExpression)uexpr.Operand : (MemberExpression)propertyExpr.Body;
        return (PropertyInfo)expr.Member;
    }
}
