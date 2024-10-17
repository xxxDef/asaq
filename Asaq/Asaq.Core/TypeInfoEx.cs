using System.Collections;
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
        return op?.IsSpecialName ?? false;
    }
    public static bool HasGreaterThanOrEqual(this Type t)
    {
        var op = t.GetUnderlineNonNullableType().GetMethod("op_GreaterThanOrEqual");
        return op?.IsSpecialName ?? false;
    }

    public static Type GetUnderlineNonNullableType(this Type t) => Nullable.GetUnderlyingType(t) ?? t;
    
    public static bool IsEnumerable(this PropertyInfo p) => p.PropertyType.IsEnumerable();
    
    public static bool IsEnumerable(this Type t) => typeof(IEnumerable).IsAssignableFrom(t);
    
    public static bool IsString(this PropertyInfo prop) => prop.PropertyType == typeof(string);
    
    public static bool IsSimpleTypeOrString(this Type type) => type.IsValueType || type.IsString();
    
    public static bool IsString(this Type type) => type == typeof(string);
}
