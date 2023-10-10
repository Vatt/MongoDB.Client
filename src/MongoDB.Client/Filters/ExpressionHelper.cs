using System.Linq.Expressions;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client.Filters
{
    internal class ExpressionHelper
    {
        public static string? GetPropertyName<TIn, TOut>(Expression<Func<TIn, TOut>> expr) where TIn : IBsonSerializer<TIn>
        {
            var body = expr.Body;

            if (body is not MemberExpression memberExpr)
            {
                return null;
            }

            return memberExpr.Member.Name;
        }
        public static string? GetPropertyName(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression memberExpr:
                    return memberExpr.Member.Name;
                default:
                    return null;
            }
        }
        public static Filter BuildFilter<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
        {
            if (expr.Parameters.Count > 1)
            {
                throw new NotSupportedException("Multi parameters not supported");
            }

            var builder = new ExpressionFilterBuilder(expr.Parameters[0]);

            Find(expr.Body, builder);

            return builder.Build();
        }
        private static void Find(Expression expr, ExpressionFilterBuilder builder)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var binExpr = (BinaryExpression)expr;

                    builder.AddExpression(binExpr);

                    Find(binExpr.Left, builder);
                    Find(binExpr.Right, builder);

                    return;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThanOrEqual:
                    binExpr = (BinaryExpression)expr;

                    builder.AddExpression(binExpr);

                    return;

                case ExpressionType.Call:
                    builder.AddExpression(expr);

                    return;
            }

            throw new NotSupportedException($"Not supported expression - {expr}");
        }
    }
}
