using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Filters
{
    internal class ExpressionFilterBuilder
    {
        private readonly ParameterExpression _parameter;
        private ExpressionType? _lastAggregateType;
        private Stack<Filter> _stack = new();
        private IReadOnlyDictionary<string, string> _mapping;
        public ExpressionFilterBuilder(ParameterExpression parameter, IReadOnlyDictionary<string, string> mapping)
        {
            _parameter = parameter;
            _mapping = mapping;
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
                        _stack.Push(new AggregateFilter(AggregateFilterType.And));
                        _lastAggregateType = ExpressionType.AndAlso;
                    }

                    break;
                case ExpressionType.OrElse:
                    if (_lastAggregateType != ExpressionType.OrElse)
                    {
                        _stack.Push(new AggregateFilter(AggregateFilterType.Or));
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
            var property = FilterVisitor.GetPropertyName(propertyExpr);

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
            var operation = binExpr.NodeType;

            Expression? propertyExpr;
            Expression? valueExpr;
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

            var property = FilterVisitor.GetPropertyName(propertyExpr);        

            if (value is null || property is null)
            {
                throw new NotSupportedException($"Not supported expression {binExpr}");
            }

            if (_mapping.TryGetValue(property, out var mappedProperty) is true)
            {
                property = mappedProperty;
            }

            return operation switch
            {
                ExpressionType.Equal => MakeEqual(property, value),
                ExpressionType.LessThan => MakeSimpleFilter(property, value, FilterType.Lt),
                ExpressionType.LessThanOrEqual => MakeSimpleFilter(property, value, FilterType.Lte),
                ExpressionType.GreaterThan => MakeSimpleFilter(property, value, FilterType.Gt),
                ExpressionType.GreaterThanOrEqual => MakeSimpleFilter(property, value, FilterType.Gte),
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
                case string[] str: return new RangeFilter<string>(propertyName, str, RangeFilterType.In);
                case int[] int32: return new RangeFilter<int>(propertyName, int32, RangeFilterType.In);
                case long[] int64: return new RangeFilter<long>(propertyName, int64, RangeFilterType.In);
                case double[] doubleValue: return new RangeFilter<double>(propertyName, doubleValue, RangeFilterType.In);
                case decimal[] decimalValue: return new RangeFilter<decimal>(propertyName, decimalValue, RangeFilterType.In);
                case BsonObjectId[] objectId: return new RangeFilter<BsonObjectId>(propertyName, objectId, RangeFilterType.In);
                case BsonTimestamp[] timestamp: return new RangeFilter<BsonTimestamp>(propertyName, timestamp, RangeFilterType.In);
                case DateTimeOffset[] dt: return new RangeFilter<DateTimeOffset>(propertyName, dt, RangeFilterType.In);
                case Guid[] guid: return new RangeFilter<Guid>(propertyName, guid, RangeFilterType.In);
                case BsonDocument[] document: return new RangeFilter<BsonDocument>(propertyName, document, RangeFilterType.In);
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
        private static Filter MakeSimpleFilter(string propertyName, object? value, FilterType op)
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
