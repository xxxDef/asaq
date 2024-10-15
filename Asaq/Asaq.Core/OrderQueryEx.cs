using System.Linq.Expressions;

namespace Asaq.Core;

public static class OrderQueryEx
{
    public static IQueryable<T> ApplyOrder<T, TOrder>(this IQueryable<T> source, OrderQuery<TOrder>? sortQuery) 
        where TOrder : class
    {
        if (sortQuery == null)
            return source;
        var allFields = GetFields(sortQuery.SortAsc, true)
            .Union(GetFields(sortQuery.SortDesc, false))
            .OrderBy(f => f.value);

        var first = true;
        foreach (var pos in allFields)
        {
            var property = typeof(T).GetProperty(pos.name);
            InvalidOperation.IfNull(property, $"Property {pos.name} defined in {typeof(TOrder)} not found in {typeof(T)}");
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            // create type "Func<T, TKey>" where TKey is type of field which is used for ordering.
            // this type will be used as parameter for OrderBy or OrderByDescending method
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType);

            var orderByExpression = Expression.Lambda(delegateType, propertyAccess, parameter);

            var methodName = first
                ? pos.asc ? "OrderBy" : "OrderByDescending"
                : pos.asc ? "ThenBy" : "ThenByDescending";

            source = CallQueryableOrderByMethod(source, methodName, property.PropertyType, orderByExpression);
        }

        return source;
    }

    static IEnumerable<(string name, int value, bool asc)> GetFields<TOrder>(TOrder? sort, bool asc) where TOrder : class
    {
        if (sort is null)
            yield break;

        var props = typeof(TOrder).GetProperties();

        foreach(var p in props)
        {
            var value = p.GetValue(sort) as int?;
            if (value == null)
                continue;
            yield return (name: p.Name, value: value.Value, asc);
        }
    }

    static IQueryable<T> CallQueryableOrderByMethod<T>(IQueryable<T> source, string methodName, Type propertyType, LambdaExpression orderByExpression)
    {
        // we have no ability to call OrderBy or OrderByDescending directly because 
        // we have no TKey defined anywhere
        // therefore we have to use reflection to call it

        ////////////////////////////////////////////////
        // source.OrderBy<T,propertyType>(orderByExpression)
        // where orderByExpression is Func<T, propertyType>
        ////////////////////////////////////////////////

        var method = typeof(Queryable).GetMethods()
            .Single(method => method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(T), propertyType);

        var res = genericMethod.Invoke(null, [source, orderByExpression]);

        InvalidOperation.IfNull(res);

        return (IOrderedQueryable<T>)res;
    }
}
