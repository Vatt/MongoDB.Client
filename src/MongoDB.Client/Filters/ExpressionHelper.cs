using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using Sprache;

namespace MongoDB.Client.Filters
{
    internal class ExpressionHelper
    {
        private record struct Context(ContainerizedFilter? Last, ExpressionType LastType, Stack<Filter> Filters, Stack<Expression> Stack, ParameterExpression Parameter);
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
        public static Filter ParseExpression<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
        {
            if (expr.Parameters.Count > 1)
            {
                throw new NotSupportedException("Multi parameters not supported");
            }
            var ctx = new Context(null, default, new(), new(), expr.Parameters[0]);

            //Parse(expr.Body, ref ctx);
            VisitExpr(expr.Body, ref ctx);

            return default;
        }
        private static void VisitExpr(Expression expr, ref Context ctx)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var binExpr = (BinaryExpression)expr;
                    var filer = MakeLogical(expr.NodeType);
                    ctx.Filters.Push(filer);

                    VisitExpr(binExpr.Left, ref ctx);
                    VisitExpr(binExpr.Right, ref ctx);

                    break;
                case ExpressionType.Equal:
                    binExpr = (BinaryExpression)expr;
                    ctx.Stack.Push(binExpr);

                    if (binExpr.Right is ConstantExpression rightConstExpr)
                    {
                        ctx.Stack.Push(binExpr.Left);
                        VisitConstantExpr(rightConstExpr, ref ctx);

                        break;
                    }
                    else if (binExpr.Left is ConstantExpression leftConstExpr)
                    {
                        ctx.Stack.Push(binExpr.Right);
                        VisitConstantExpr(leftConstExpr, ref ctx);

                        break;
                    }
                    else if (binExpr.Right is MemberExpression rightMemberExpr && binExpr.Left is MemberExpression leftMemberExpr && leftMemberExpr.Expression == ctx.Parameter)
                    {
                        ctx.Stack.Push(rightMemberExpr);
                        ctx.Stack.Push(binExpr.Left);
                        VisitMemberExpr(rightMemberExpr, ref ctx);

                        break;
                    }
                    else if (binExpr.Right is MemberExpression rightMemberExpr1 && binExpr.Left is MemberExpression leftMemberExpr1 && rightMemberExpr1.Expression == ctx.Parameter)
                    {
                        ctx.Stack.Push(leftMemberExpr1);
                        ctx.Stack.Push(binExpr.Right);
                        VisitMemberExpr(leftMemberExpr1, ref ctx);

                        break;
                    }

                    throw new NotSupportedException($"Unsupported Expression {expr}");

                case ExpressionType.Call:
                    ctx.Stack.Push(expr);
                    VisitCallExpr((MethodCallExpression)expr, ref ctx);

                    break;
                case ExpressionType.MemberAccess:
                    ctx.Stack.Push(expr);
                    VisitMemberExpr((MemberExpression)expr, ref ctx);

                    break;
            }
        }
        private static void VisitCallExpr(MethodCallExpression callExpr, ref Context ctx)
        {
            var methodName = callExpr.Method.Name;
            if (callExpr.Method.Name != "Contains")
            {
                throw new NotSupportedException($"Unsupported MethodCallExpression with method name {methodName}");
            }
            var arg1 = callExpr.Arguments[0];
            var arg2 = callExpr.Arguments[1];

            ctx.Stack.Push(arg1);
            ctx.Stack.Push(arg2);
            VisitMemberExpr((MemberExpression)arg1, ref ctx);
        }
        private static void VisitMemberExpr(MemberExpression memberExpr, ref Context ctx)
        {
            if (memberExpr.Expression is ConstantExpression)
            {
                VisitConstantExpr((ConstantExpression)memberExpr.Expression, ref ctx);

                return;
            }
            else if (ctx.Stack.Peek() is BinaryExpression binExpr && binExpr.Right is ConstantExpression)
            {
                 ctx.Stack.Push(memberExpr);
                VisitConstantExpr((ConstantExpression)binExpr.Left, ref ctx);

                return;
            }

            throw new NotSupportedException($"Unsupported MethodCallExpression with method name {memberExpr}");
        }

        private static void VisitConstantExpr(ConstantExpression constExpr, ref Context ctx)
        {
            var stack = ctx.Stack;
            var propertyExpr = stack.Pop();
            string? closureName = null;

            if (stack.TryPeek(out var expr) && expr is MemberExpression)
            {
                closureName = (stack.Pop() as MemberExpression)!.Member.Name;
            }

            var propertyName = GetPropertyName(propertyExpr);
            
            if (propertyName == null)
            {
                throw new NotSupportedException($"Cant get property name from expression - {propertyExpr}");
            }

            object? value = null;

            if (closureName != null)
            {
                value = constExpr.Value!.GetType().GetField(closureName)!.GetValue(constExpr.Value);
            }
            else
            {
                value = constExpr.Value;
            }

            switch (stack.Pop())
            {
                case MethodCallExpression callExpr:
                    ctx.Filters.Push(MakeIn(propertyName, value));

                    break;
                case BinaryExpression binExpr:
                    ctx.Filters.Push(MakeFilter(propertyName, binExpr.NodeType, value));

                    break;

            }

        }

        private static Filter MakeFilter(string propertyName, ExpressionType op, object? value, string? innerName = null)
        {
            switch (op)
            {
                case ExpressionType.Equal:
                    return MakeEqual(propertyName, value, innerName);
                case ExpressionType.LessThan:
                    return MakeLessThan(propertyName, value, innerName);

                default:
                    throw new NotSupportedException($"Unsupported ExpressionType - {op}");
            }  

        }
        private static ContainerizedFilter MakeLogical(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.OrElse:
                    return new OrFilter();
                case ExpressionType.AndAlso:
                    return new AndFilter();
                default:
                    throw new NotSupportedException($"Unsupported expression type for logical filter - {op}");
            }
        }
        private static Filter MakeEqual(string propertyName, object? value, string? innerName = null)
        {
            if (value is null)
            {
                return new EqFilter<object>(propertyName, null);
            }
            switch (value)
            {
                case string str: return new EqFilter<string>(propertyName, str);
                case int int32: return new EqFilter<int>(propertyName, int32);
                case long int64: return new EqFilter<long>(propertyName, int64);
                case double doubleValue: return new EqFilter<double>(propertyName, doubleValue);
                case decimal decimalValue: return new EqFilter<decimal>(propertyName, decimalValue);
                case BsonObjectId objectId: return new EqFilter<BsonObjectId>(propertyName, objectId);
                case BsonTimestamp timestamp: return new EqFilter<BsonTimestamp>(propertyName, timestamp);
                case DateTimeOffset dt: return new EqFilter<DateTimeOffset>(propertyName, dt);
                case Guid guid: return new EqFilter<Guid>(propertyName, guid);
                case BsonDocument document: return Filter.Document(document);
            }

            if (innerName is null)
            {
                throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
            }

            return MakeEqual(propertyName, value.GetType().GetField(innerName)!.GetValue(value));
        }
        private static Filter MakeLessThan(string propertyName, object? value, string? innerName = null)
        {
            if (value is null)
            {
                return new LtFilter<object>(propertyName, null);
            }
            switch (value)
            {
                case string str: return new LtFilter<string>(propertyName, str);
                case int int32: return new LtFilter<int>(propertyName, int32);
                case long int64: return new LtFilter<long>(propertyName, int64);
                case double doubleValue: return new LtFilter<double>(propertyName, doubleValue);
                case decimal decimalValue: return new LtFilter<decimal>(propertyName, decimalValue);
                case BsonObjectId objectId: return new LtFilter<BsonObjectId>(propertyName, objectId);
                case BsonTimestamp timestamp: return new LtFilter<BsonTimestamp>(propertyName, timestamp);
                case DateTimeOffset dt: return new LtFilter<DateTimeOffset>(propertyName, dt);
                case Guid guid: return new LtFilter<Guid>(propertyName, guid);
                case BsonDocument document: return Filter.Document(document);
            }

            if (innerName is null)
            {
                throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
            }

            return MakeLessThan(propertyName, value.GetType().GetField(innerName)!.GetValue(value));
        }
        private static Filter MakeIn(string propertyName, object? value, string? innerName = null)
        {
            if (value is null)
            {
                //return new LtFilter<object>(propertyName, null);
            }
            switch (value)
            {
                case string[] str: return new InFilter<string>(propertyName, str);
                case int[] int32: return new InFilter<int>(propertyName, int32);
                case long[] int64: return new InFilter<long>(propertyName, int64);
                case double[] doubleValue: return new InFilter<double>(propertyName, doubleValue);
                case decimal[] decimalValue: return new InFilter<decimal>(propertyName, decimalValue);
                case BsonObjectId[] objectId: return new InFilter<BsonObjectId>(propertyName, objectId);
                case BsonTimestamp[] timestamp: return new InFilter<BsonTimestamp>(propertyName, timestamp);
                case DateTimeOffset[] dt: return new InFilter<DateTimeOffset>(propertyName, dt);
                case Guid[] guid: return new InFilter<Guid>(propertyName, guid);
                case BsonDocument[] document: return new InFilter<BsonDocument>(propertyName, document);
            }

            if (innerName is null)
            {
                throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
            }

            return MakeIn(propertyName, value.GetType().GetField(innerName)!.GetValue(value));
        }
    }
}
