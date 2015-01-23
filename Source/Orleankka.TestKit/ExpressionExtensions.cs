using System;
using System.Linq;
using System.Linq.Expressions;

namespace Orleankka.TestKit
{
    public static class ExpressionExtensions
    {
        public static Expression ApplyParameter(this Expression expression, object arg)
        {
            return new ApplyParameterVisitor(arg).Visit(expression);
        }
    }

    class ApplyParameterVisitor : ExpressionVisitor
    {
        private readonly object arg;

        public ApplyParameterVisitor(object arg)
        {
            this.arg = arg;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Constant(arg);
        }
    }
}
