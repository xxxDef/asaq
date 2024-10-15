namespace Asaq.Core;

public static class WhereQueryEx
{
    public static IQueryable<T> ApplyWhere<T, TFields>(this IQueryable<T> query, WhereQuery<TFields>? where)
    where TFields : class
    {
        var expresson = where?.BuildExpression<T,TFields>(); 
        return expresson is null ? query : query.Where(expresson);
    }
}
