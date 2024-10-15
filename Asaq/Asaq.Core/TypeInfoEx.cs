using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Asaq.Core;

public static class TypeInfoEx
{
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

    public static Type GetUnderlineNonNullableType(this Type t)
    {
        return Nullable.GetUnderlyingType(t) ?? t;
    }

    public static bool IsAssociativeDictionary(this PropertyInfo p)
    {
        return p.PropertyType.IsAssociativeDictionary();
    }
    public static bool IsCollection(this PropertyInfo p)
    {
        return p.PropertyType.IsCollection();
    }
    public static bool IsCollection(this Type t)
    {
        return t.IsGenericType && typeof(ICollection<>).IsAssignableFrom(t.GetGenericTypeDefinition());
    }
    public static bool IsEnumerable(this PropertyInfo p)
    {
        return p.PropertyType.IsEnumerable();
    }
    public static bool IsEnumerable(this Type t)
    {
        return typeof(IEnumerable).IsAssignableFrom(t);
    }
    public static bool IsAssociativeDictionary(this Type t)
    {
        return t == typeof(IDictionary<string, object>);
    }

    public static Type GetItemType(object collection)
    {
        var type = collection.GetType();
        return type.GetItemType();
    }

    public static Type GetItemType(this Type type)
    {
        if (type.IsArray)
        {
            var res = type.GetElementType();
            InvalidOperation.IfNull(res);
            return res;
        }
        InvalidOperation.IfFalse(type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type), $"{type} is not enumerable");
        return type.GetGenericArguments()[0];
    }

    public static bool IsSimpleTypeOrString(this PropertyInfo prop)
    {
        return prop.PropertyType.IsValueType || prop.IsString();
    }

    public static bool IsArrayOfSimpleTypeOrString(this PropertyInfo prop)
    {
        return prop.PropertyType.IsArrayOfSimpleTypeOrString();
    }

    public static bool IsArrayOfSimpleTypeOrString(this Type type)
    {
        if (!type.IsArray)
            return false;

        var elementType = type.GetElementType();
        InvalidOperation.IfNull(elementType);

        return elementType.IsSimpleTypeOrString();
    }

    public static bool IsEnumerableOfSimpleTypeOrString(this PropertyInfo prop)
    {
        if (!prop.IsEnumerable())
            return false;

        var elementType = prop.PropertyType;
        InvalidOperation.IfNull(elementType);

        return elementType.IsSimpleTypeOrString();
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
