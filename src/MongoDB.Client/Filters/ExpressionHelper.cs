using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client.Filters
{
    internal class ExpressionHelper
    {
        private record struct Context(ContainerizedFilter? Last, ExpressionType LastType, Stack<Filter> Stack);
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
            var ctx = new Context(null, default, new());

            Parse(expr.Body, ref ctx);

            return default;
        }
        private static void Parse(Expression expr, ref Context ctx)
        {
            switch (expr)
            {
                case BinaryExpression simpleBinExpr when simpleBinExpr.Left is not BinaryExpression && simpleBinExpr.Right is not BinaryExpression:
                    var property = GetPropertyName(simpleBinExpr.Left);

                    if (property == null)
                    {
                        throw new NotSupportedException($"Cant get property name from expression - {simpleBinExpr.Left}");
                    }

                    if (simpleBinExpr.Right is not not UnaryExpression)
                    {
                        var newFilter = VisitValue(property, simpleBinExpr.NodeType, simpleBinExpr.Right);
                        ctx.Stack.Push(newFilter);

                        return;
                    }

                    throw new NotSupportedException($"Unsupported expression - {expr}");

                case BinaryExpression binExpr:
                                      
                    if (ctx.Last is null || ctx.LastType != binExpr.NodeType)
                    {
                        var newLast = MakeLogical(binExpr.NodeType);
                        ctx.Stack.Push(newLast);
                        ctx.Last = newLast;
                        ctx.LastType = binExpr.NodeType;
                    }                    

                    Parse(binExpr.Right, ref ctx);
                    Parse(binExpr.Left, ref ctx);

                    break;
                case MethodCallExpression callExpr:
                    VisitCallExpr(callExpr, ref ctx);

                    break;
                default:
                    throw new NotSupportedException($"Unsupported expression - {expr}");
            }
        }
        private static void VisitBinaryExpr(BinaryExpression binExpr, ref Context ctx)
        {
            if (binExpr.Left is not BinaryExpression &&  binExpr.Right is not BinaryExpression)
            {

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
            var propertyName = GetPropertyName(arg2);
            VisitMemberExpr(propertyName, (MemberExpression)arg1, ref ctx);
        }
        private static void VisitMemberExpr(string propertyName, MemberExpression memberExpr, ref Context ctx)
        {
            if (memberExpr.Expression is not ConstantExpression)
            {
                throw new NotSupportedException($"Unsupported MemberExpression - {memberExpr}");
            }

            VisitConstantExpr(propertyName, memberExpr, (ConstantExpression)memberExpr.Expression, ref ctx);
        }

        private static void VisitConstantExpr(string propertyName, MemberExpression owner, ConstantExpression expr, ref Context ctx)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Equal:
                    ctx.Stack.Push(MakeEqual(propertyName, expr.Value, owner.Member.Name));

                    break;
                case ExpressionType.LessThan:
                    ctx.Stack.Push(MakeLessThan(propertyName, expr.Value, owner.Member.Name));

                    break;
                case ExpressionType.Constant:
                    ctx.Stack.Push(MakeIn(propertyName, expr.Value, owner.Member.Name));

                    break;
            }
        }

        private static Filter VisitValue(string propertyName, ExpressionType op, Expression expr, string innerName = null)
        {
            switch (expr)
            {
                case ConstantExpression constExpr:
                    return MakeFilter(propertyName, op, constExpr.Value, innerName);
                case MemberExpression memberExpr:
                    return VisitValue(propertyName, op, memberExpr.Expression!, memberExpr.Member.Name);
            }

            throw new NotSupportedException($"Unsupported expression - {expr}");
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
