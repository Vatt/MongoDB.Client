using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Filters
{
    internal class ExpressionFilterBuilder
    {
        private readonly ParameterExpression _parameter;
        private ExpressionType? _lastAggregateType;
        private Stack<Filter> _stack = new();
        public ExpressionFilterBuilder(ParameterExpression parameter)
        {
            _parameter = parameter;
        }
        public Filter Build()
        {
            if (_stack.Count is 1)
            {
                return _stack.Pop();
            }
            else
            {
                Filter? result = null;
                List<Filter> filters = new();

                while (_stack.TryPop(out var filter))
                {
                    if (filter is not AggregateFilter aggregateFilter)
                    {
                        filters.Add(filter);
                    }
                    else
                    {
                        aggregateFilter.AddRange(filters);

                        if (result is null)
                        {
                            result = aggregateFilter;
                        }
                        else
                        {
                            aggregateFilter.Add(result);
                            result = aggregateFilter;
                        }
                        filters = new();
                    }
                }

                if (result is null)
                {
                    throw new InvalidOperationException("Can't build result filter");
                }

                return result;
            }
        }
        public void AddExpression(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    _stack.Push(MakeSimpleFilter((BinaryExpression)expr));

                    break;
                case ExpressionType.Call:
                    _stack.Push(MakeCallFilter((MethodCallExpression)expr));

                    break;
                case ExpressionType.AndAlso:
                    if (_lastAggregateType != ExpressionType.AndAlso)
                    {
                        _stack.Push(new AndFilter());
                        _lastAggregateType = ExpressionType.AndAlso;
                    }

                    break;
                case ExpressionType.OrElse:
                    if (_lastAggregateType != ExpressionType.OrElse)
                    {
                        _stack.Push(new OrFilter());
                        _lastAggregateType = ExpressionType.OrElse;
                    }


                    break;

            }
        }
        private Filter MakeCallFilter(MethodCallExpression callExpr)
        {
            var methodName = callExpr.Method.Name;
            if (callExpr.Method.Name != "Contains")
            {
                throw new NotSupportedException($"Not supported MethodCallExpression with method name {methodName}");
            }
            var valueExpr = callExpr.Arguments[0];
            var propertyExpr = callExpr.Arguments[1];

            object? value = null;
            var property = ExpressionHelper.GetPropertyName(propertyExpr);

            if (valueExpr is ConstantExpression constExpr)
            {
                value = constExpr.Value;
            }
            else if (valueExpr is MemberExpression memberExpr)
            {
                var closureName = memberExpr.Member.Name;
                constExpr = (ConstantExpression)memberExpr.Expression!;
                value = constExpr.Value!.GetType().GetField(closureName)!.GetValue(constExpr.Value);
            }

            if (value is null || property is null)
            {
                throw new NotSupportedException($"Not supported expression {callExpr}");
            }

            return MakeIn(property, value);

        }
        private Filter MakeSimpleFilter(BinaryExpression binExpr)
        {
            Expression? propertyExpr = null;
            Expression? valueExpr = null;
            var operation = binExpr.NodeType;

            if (binExpr.Left is MemberExpression leftMember && leftMember.Expression == _parameter)
            {
                propertyExpr = binExpr.Left;
                valueExpr = binExpr.Right;
            }
            else
            {
                propertyExpr = binExpr.Right;
                valueExpr = binExpr.Left;
            }

            var property = ExpressionHelper.GetPropertyName(propertyExpr);
            object? value = null;

            if (valueExpr is ConstantExpression constExpr)
            {
                value = constExpr.Value;
            }
            else if (valueExpr is MemberExpression memberExpr)
            {
                var closureName = memberExpr.Member.Name;
                constExpr = (ConstantExpression)memberExpr.Expression!;
                value = constExpr.Value!.GetType().GetField(closureName)!.GetValue(constExpr.Value);
            }

            if (value is null || property is null)
            {
                throw new NotSupportedException($"Not supported expression {binExpr}");
            }

            return operation switch
            {
                ExpressionType.Equal => MakeEqual(property, value),
                ExpressionType.LessThan => MakeSimpleFilter(property, value, FilterOp.Lt),
                ExpressionType.LessThanOrEqual => MakeSimpleFilter(property, value, FilterOp.Lte),
                ExpressionType.GreaterThan => MakeSimpleFilter(property, value, FilterOp.Gt),
                ExpressionType.GreaterThanOrEqual => MakeSimpleFilter(property, value, FilterOp.Gte),
                _ => throw new NotSupportedException($"Not supported  operation {operation}")
            };
        }
        private static Filter MakeIn(string propertyName, object? value)
        {
            if (value is null)
            {
                //TODO: FIX IT
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

            throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
        }
        static Filter MakeEqual(string propertyName, object? value)
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

            throw new NotSupportedException($"Unsupported type in Expression(equal filter) - {value.GetType()}");
        }
        private static Filter MakeSimpleFilter(string propertyName, object? value, FilterOp op)
        {
            if (value is null)
            {
                return new Filter<object>(propertyName, null, op);
            }
            switch (value)
            {
                case string str: return new Filter<string>(propertyName, str, op);
                case int int32: return new Filter<int>(propertyName, int32, op);
                case long int64: return new Filter<long>(propertyName, int64, op);
                case double doubleValue: return new Filter<double>(propertyName, doubleValue, op);
                case decimal decimalValue: return new Filter<decimal>(propertyName, decimalValue, op);
                case BsonObjectId objectId: return new Filter<BsonObjectId>(propertyName, objectId, op);
                case BsonTimestamp timestamp: return new Filter<BsonTimestamp>(propertyName, timestamp, op);
                case DateTimeOffset dt: return new Filter<DateTimeOffset>(propertyName, dt, op);
                case Guid guid: return new Filter<Guid>(propertyName, guid, op);
            }

            throw new NotSupportedException($"Unsupported type in Expression - {value.GetType()}");
        }
    }
}
