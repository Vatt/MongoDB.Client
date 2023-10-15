//using System.Linq.Expressions;
//using MongoDB.Client.Bson.Serialization;
//using MongoDB.Client.Bson.Writer;
//using MongoDB.Client.Exceptions;

//namespace MongoDB.Client.Filters
//{
//    internal class FilterVisitor
//    {
//        public static string? GetPropertyName<TIn, TOut>(Expression<Func<TIn, TOut>> expr) where TIn : IBsonSerializer<TIn>
//        {
//            var body = expr.Body;

//            if (body is not MemberExpression memberExpr)
//            {
//                return null;
//            }

//            return memberExpr.Member.Name;
//        }

//        public static Filter BuildFilter<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
//        {
//            if (expr.Parameters.Count > 1)
//            {
//                return ThrowHelper.Expression<Filter>($"Multi parameters not supported, parameters {string.Join(',', expr.Parameters)}");
//            }

//            var builder = new ExpressionFilterBuilder(expr.Parameters[0], MappingProvider<T>.Mapping);

//            Find(expr.Body, builder);

//            return builder.Build();
//        }

//    }
//}
