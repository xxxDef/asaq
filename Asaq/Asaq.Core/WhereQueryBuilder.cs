using System.Linq.Expressions;

namespace Asaq.Core;

public static class WhereQueryBuilder
{
    public static IQueryable<T> ApplyWhere<T>(this IQueryable<T> query, IWhereQuery? where)
    {
        var expresson = where?.BuildExpression<T>();
        return expresson is null ? query : query.Where(expresson);
    }

    public static Expression<Func<T,bool>>? BuildExpression<T>(this IWhereQuery where)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var expressons = where.CreateExpressions<T>(param).ToArray();

        if (expressons.Length == 0)
            return null;

        var body = expressons.Aggregate(Expression.AndAlso);
        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return lambda;
    }
}
