using System.Linq.Expressions;

namespace Asaq.Core;

public interface IWhereQuery
{
    IEnumerable<Expression> CreateExpressions(ParameterExpression input);
}
