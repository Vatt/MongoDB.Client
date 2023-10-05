using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;
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
        public static Filter ParseExpression<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
        {
            return Parse(expr.Body);
        }
        private static Filter Parse(Expression expr)
        {
            switch (expr)
            {
                case BinaryExpression simpleBinExpr when simpleBinExpr.Left is not BinaryExpression && simpleBinExpr.Right is not BinaryExpression:
                    var property = GetPropertyName(simpleBinExpr.Left);

                    if (property == null)
                    {
                        throw new NotSupportedException($"Cant get property name from expression - {simpleBinExpr.Left}");
                    }

                    if (simpleBinExpr.Right is not BinaryExpression and not UnaryExpression)
                    {
                        return VisitValue(property, simpleBinExpr.NodeType, simpleBinExpr.Right);
                    }

                    throw new NotSupportedException($"Unsupported expression - {expr}");

                case BinaryExpression binExpr when binExpr.Left is BinaryExpression:
                    var logicalFilter = MakeLogical(binExpr.NodeType);

                    ParsePrivate(binExpr.Right, logicalFilter);
                    ParsePrivate(binExpr.Left, logicalFilter);

                    return logicalFilter;                   
                default:
                    throw new NotSupportedException($"Unsupported expression - {expr}");
            }
        }
        private static void ParsePrivate(Expression expr, LogicalFilter owner)
        {
            switch (expr)
            {
                case BinaryExpression simpleBinExpr when simpleBinExpr.Left is not BinaryExpression && simpleBinExpr.Right is not BinaryExpression:
                    var property = GetPropertyName(simpleBinExpr.Left);

                    if (property == null)
                    {
                        throw new NotSupportedException($"Cant get property name from expression - {simpleBinExpr.Left}");
                    }

                    if (simpleBinExpr.Right is not BinaryExpression and not UnaryExpression)
                    {
                        var value =  VisitValue(property, simpleBinExpr.NodeType, simpleBinExpr.Right);
                        owner.Add(value);

                        return;
                    }

                    if (simpleBinExpr.Right is BinaryExpression or UnaryExpression)
                    {
                        var value = Parse(simpleBinExpr.Right);
                        owner.Add(value);

                        return;
                    }

                    throw new NotSupportedException($"Unsupported expression - {expr}");

                case BinaryExpression binExpr when binExpr.Left is BinaryExpression:
                    var logicalFilter = MakeLogical(binExpr.NodeType);

                    ParsePrivate(binExpr.Right, logicalFilter);
                    ParsePrivate(binExpr.Left, logicalFilter);

                    break;
                default:
                    throw new NotSupportedException($"Unsupported expression - {expr}");
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
        private static LogicalFilter MakeLogical(ExpressionType op)
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
                return new LessThanFilter<object>(propertyName, null);
            }
            switch (value)
            {
                case string str: return new LessThanFilter<string>(propertyName, str);
                case int int32: return new LessThanFilter<int>(propertyName, int32);
                case long int64: return new LessThanFilter<long>(propertyName, int64);
                case double doubleValue: return new LessThanFilter<double>(propertyName, doubleValue);
                case decimal decimalValue: return new LessThanFilter<decimal>(propertyName, decimalValue);
                case BsonObjectId objectId: return new LessThanFilter<BsonObjectId>(propertyName, objectId);
                case BsonTimestamp timestamp: return new LessThanFilter<BsonTimestamp>(propertyName, timestamp);
                case DateTimeOffset dt: return new LessThanFilter<DateTimeOffset>(propertyName, dt);
                case Guid guid: return new LessThanFilter<Guid>(propertyName, guid);
                case BsonDocument document: return Filter.Document(document);
            }

            if (innerName is null)
            {
                throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
            }

            return MakeLessThan(propertyName, value.GetType().GetField(innerName)!.GetValue(value));
        }
    }
}
