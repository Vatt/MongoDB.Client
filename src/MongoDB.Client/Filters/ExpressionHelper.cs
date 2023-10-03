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
        public static BsonDocument ParseExpression<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
        {
            return Parse(expr.Body);
        }
        private static BsonDocument Parse(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    switch (expr)
                    {
                        case BinaryExpression binExpr when binExpr.Left is MemberExpression prop && binExpr.Right is not BinaryExpression:
                            var member = VisitMember(prop);
                            var (type, value) = VisitValue(binExpr.Right);
                            BsonDocument doc = new();
                            var op = expr.NodeType switch
                            {
                                ExpressionType.Equal => "$eq",
                                _ => throw new NotImplementedException(expr.NodeType.ToString())
                            };
                            var element = BsonElement.Create(doc, type, op, value);
                            doc.Add(element);
                            var filter = new BsonDocument(member, doc);
                            return filter;
                    }
                    break;
            }

            return null;
        }
        private static void BinOperation(BinaryExpression expr)
        {

        }
        private static string VisitMember(MemberExpression expr)
        {
            return expr.Member.Name;
        }
        private static (BsonType, object?) VisitValue(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Constant when expr is ConstantExpression constExpr:
                    return (GetBsonType(constExpr.Value), constExpr.Value);
                case ExpressionType.MemberAccess when expr is MemberExpression memberExpr:
                    return (GetBsonType(((ConstantExpression)memberExpr.Expression).Value), ((ConstantExpression)memberExpr.Expression).Value);
                default:
                    return default;
            }
        }
        private static BsonType GetBsonType(object? value)
        {
            if (value is null)
            {
                return BsonType.Null;
            }

            return value switch
            {
                string => BsonType.String,
                int => BsonType.Int32,
                long => BsonType.Int64,
                double => BsonType.Double,
                decimal => BsonType.Decimal,
                BsonObjectId => BsonType.ObjectId,
                _ => throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}")
            };
        }
    }
}
