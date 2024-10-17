using System.Linq.Expressions;

namespace Asaq.Core;

public interface IWhereQuery
{
    IEnumerable<Expression> CreateExpressions<T>(ParameterExpression input);
}
