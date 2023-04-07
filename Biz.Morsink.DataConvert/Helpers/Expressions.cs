using System.Linq.Expressions;

namespace Biz.Morsink.DataConvert.Helpers
{
    static class Expressions
    {
        private class ReplaceVisitor : ExpressionVisitor
        {
            public ReplaceVisitor(ParameterExpression original, Expression replacement)
            {
                Original = original;
                Replacement = replacement;
            }

            public ParameterExpression Original { get; }
            public Expression Replacement { get; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node.Equals(Original) ? Replacement : base.VisitParameter(node);
            }
        }
        /// <summary>
        /// Applies an expression as argument to a lambda.
        /// </summary>
        /// <param name="lambda">A lambda expression with 1 parameter</param>
        /// <param name="ex">The expression to use as input for the lambda expression</param>
        /// <returns>The expanded body of the LambdaExpression.</returns>
        public static Expression ApplyTo(this LambdaExpression lambda, Expression ex)
        {
            var visitor = new ReplaceVisitor(lambda.Parameters[0], ex);
            return visitor.Visit(lambda.Body);
        }
    }
}
