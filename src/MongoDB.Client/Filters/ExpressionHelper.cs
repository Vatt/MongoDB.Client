using System;
using System.Linq.Expressions;
using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using Sprache;

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
                case BinaryExpression binaryExpr:
                    var property = GetPropertyName(binaryExpr.Left);

                    if (property == null)
                    {
                        throw new NotSupportedException($"Cant get property name from expression - {binaryExpr.Left}");
                    }

                    if (binaryExpr.Right is not BinaryExpression and not UnaryExpression)
                    {
                        return VisitValue(property, binaryExpr.NodeType, binaryExpr.Right);
                    }

                    throw new NotSupportedException($"Unsupported expression - {expr}");
            }

            throw new NotSupportedException($"Unsupported expression - {expr}");
        }
        private static Filter VisitValue(string propertyName, ExpressionType op, Expression expr, string memberName = null)
        {
            switch (expr)
            {
                case ConstantExpression constExpr:
                    return MakeFilter(propertyName, op, constExpr.Value, memberName);
                case MemberExpression memberExpr:
                    return VisitValue(propertyName, op, memberExpr.Expression, memberExpr.Member.Name);
            }

            throw new NotSupportedException($"Unsupported expression - {expr}");
        }
        private static Filter MakeFilter(string propertyName, ExpressionType op, object? value, string? memberName = null)
        {
            switch (op)
            {
                case ExpressionType.Equal:
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
                    }
                    
                    if (memberName is null)
                    {
                        throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
                    }

                    return MakeFilter(propertyName, op, value.GetType().GetField(memberName).GetValue(value));


                default:
                    throw new NotSupportedException($"Unsupported ExpressionType - {op}");
            }  

        }
    }
}
