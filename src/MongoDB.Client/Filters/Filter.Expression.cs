using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDB.Client.Filters
{
    public abstract partial class Filter
    {
        private readonly record struct Context(ParameterExpression Parameter, IReadOnlyDictionary<string, string> Mapping);
        public static Filter FromExpression<T>(Expression<Func<T, bool>> expr) where T : IBsonSerializer<T>
        {
            if (expr.Parameters.Count > 1)
            {
                return ThrowHelper.Expression<Filter>($"Multi parameters not supported, parameter list {string.Join(',', expr.Parameters)}");
            }

            var ctx = new Context(expr.Parameters[0], MappingProvider<T>.Mapping);

            return Next(expr.Body, ref ctx);
        }
        private static Filter Next(Expression expr, ref Context ctx)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.AndAlso:
                    {
                        var binExpr = (BinaryExpression)expr;
                        var newAggregateFilter = new AggregateFilter(AggregateFilterType.And);
                        var right = Next(binExpr.Right, ref ctx);
                        var left = Next(binExpr.Left, ref ctx);
                        newAggregateFilter.Add(right, left);

                        return newAggregateFilter;
                    }
                case ExpressionType.OrElse:
                    {
                        var binExpr = (BinaryExpression)expr;
                        var newAggregateFilter = new AggregateFilter(AggregateFilterType.Or);
                        var right = Next(binExpr.Right, ref ctx);
                        var left = Next(binExpr.Left, ref ctx);
                        newAggregateFilter.Add(right, left);

                        return newAggregateFilter;
                    }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    {
                        var binExpr = (BinaryExpression)expr;
                        if (binExpr.Left.NodeType is ExpressionType.Call || binExpr.Right.NodeType is ExpressionType.Call)
                        {
                            return MakeRangeFilter(binExpr);
                        }
                        return MakeSimpleFilter(binExpr, ref ctx);
                    }
                case ExpressionType.Call:
                    return MakeRangeFilter((MethodCallExpression)expr, true);
            }

            return ThrowHelper.Expression<Filter>($"Not supported expression {expr}");
        }
        private static Filter MakeRangeFilter(BinaryExpression binExpr)
        {
            var nodeType = binExpr.NodeType;
            if (nodeType is not ExpressionType.Equal and not ExpressionType.NotEqual)
            {
                ThrowHelper.Expression<Filter>($"Not supported type in {binExpr}");
            }

            MethodCallExpression? callExpr;
            object? type;
            if (binExpr.Left.NodeType is ExpressionType.Call)
            {
                callExpr = (MethodCallExpression)binExpr.Left;
                type = ExtractValue(binExpr.Right);
            }
            else
            {
                callExpr = (MethodCallExpression)binExpr.Right;
                type = ExtractValue(binExpr.Left);
            }

            if (type is not bool)
            {
                ThrowHelper.Expression<Filter>($"Not supported type in {binExpr}");
            }

            var typed = (bool)type;

            return typed switch
            {
                true when nodeType is ExpressionType.Equal => MakeRangeFilter(callExpr, true),
                true when nodeType is ExpressionType.NotEqual => MakeRangeFilter(callExpr, false),
                false when nodeType is ExpressionType.Equal => MakeRangeFilter(callExpr, false),
                _ => MakeRangeFilter(callExpr, true)
            };
        }
        private static Filter MakeRangeFilter(MethodCallExpression callExpr, bool type)
        {
            var methodName = callExpr.Method.Name;

            if (callExpr.Method.Name != "Contains")
            {
                return ThrowHelper.Expression<Filter>($"Not supported MethodCallExpression with method name {methodName}");
            }

            object? value = ExtractValue(callExpr.Arguments[0]);
            var property = GetPropertyName(callExpr.Arguments[1]);

            if (value is null || property is null)
            {
                return ThrowHelper.Expression<Filter>($"Not supported expression {callExpr}");
            }

            return type ? MakeRange(property, value, RangeFilterType.In) : MakeRange(property, value, RangeFilterType.NotIn);

        }
        private static Filter MakeSimpleFilter(BinaryExpression binExpr, ref Context ctx)
        {
            var (parameter, mapping) = ctx;

            var operation = binExpr.NodeType;

            Expression? propertyExpr;
            Expression? valueExpr;

            if (binExpr.Left is MemberExpression leftMember && leftMember.Expression == parameter)
            {
                propertyExpr = binExpr.Left;
                valueExpr = binExpr.Right;
            }
            else
            {
                propertyExpr = binExpr.Right;
                valueExpr = binExpr.Left;
            }

            object? value = ExtractValue(valueExpr);

            var property = GetPropertyName(propertyExpr);

            if (value is null || property is null)
            {
                return ThrowHelper.Expression<Filter>($"Not supported expression {binExpr}");
            }

            if (mapping.TryGetValue(property, out var mappedProperty) is true)
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
                ExpressionType.NotEqual => MakeSimpleFilter(property, value, FilterType.Ne),
                _ => throw new NotSupportedException($"Not supported  operation {operation}")
            };
        }
        private static object? ExtractValue(Expression expr) => expr switch
        {
            ConstantExpression constExpr => ExtractValue(constExpr),
            MemberExpression memberExpr => ExtractValue(memberExpr),
            _ => ThrowHelper.Expression<Filter>($"Can't extract value from expression {expr}")
        };
        private static object? ExtractValue(ConstantExpression constExpr)
        {
            return constExpr.Value;
        }
        private static object? ExtractValue(MemberExpression memberExpr)
        {
            var closureName = memberExpr.Member.Name;
            ConstantExpression? constExpr;
            if (memberExpr.Expression is null)
            {
                if (memberExpr.Member is FieldInfo field)
                {
                    return field.GetValue(null);
                }
                else if (memberExpr.Member is PropertyInfo property)
                {
                    return property.GetValue(null);
                }
            }
            else if (memberExpr.Expression!.NodeType is ExpressionType.Constant)
            {
                constExpr = (ConstantExpression)memberExpr.Expression;
                var flags = GetBindingFlags(memberExpr.Member);
                var field = constExpr.Value!.GetType().GetField(closureName, flags);
                return field!.GetValue(constExpr.Value);
            }
            else if (memberExpr.Expression is MemberExpression innerExpr)
            {
                List<(string, MemberTypes, BindingFlags)> trace = new();
                while (innerExpr.Expression is not null)
                {
                    trace.Add((innerExpr.Member.Name, innerExpr.Member.MemberType, GetBindingFlags(innerExpr.Member)));

                    if (innerExpr.Expression is MemberExpression)
                    {
                        innerExpr = (MemberExpression)innerExpr.Expression;
                    }
                    else
                    {
                        break;
                    }
                }
                object? value = null;

                if (innerExpr.Expression is null)
                {
                    value = innerExpr.Member switch
                    {
                        FieldInfo field => field.GetValue(null)!,
                        PropertyInfo property => property.GetValue(null)!,
                    };
                }
                else
                {
                    constExpr = (ConstantExpression)innerExpr.Expression!;
                    value = constExpr.Value!;
                }


                trace.Reverse();

                foreach (var (field, type, flags) in trace)
                {
                    value = type switch
                    {
                        MemberTypes.Field => value!.GetType().GetField(field, flags).GetValue(value)!,
                        MemberTypes.Property => value.GetType().GetProperty(field, flags)!.GetValue(value)!
                    };
                }

                var fieldClosure = value!.GetType().GetField(closureName);

                return fieldClosure is null ? value.GetType().GetProperty(closureName)!.GetValue(value) : fieldClosure.GetValue(value);
            }

            return ThrowHelper.Expression<Filter>($"Can't extract value from expression {memberExpr}");
        }
        private static BindingFlags GetBindingFlags(MemberInfo member)
        {
            BindingFlags flags = BindingFlags.Default;
            if (member is FieldInfo field)
            {
                if (field.IsStatic)
                {
                    flags |= BindingFlags.Static;
                }
                else
                {
                    flags |= BindingFlags.Instance;
                }

                if (field.IsPublic == false)
                {
                    flags |= BindingFlags.NonPublic;
                }
                else
                {
                    flags |= BindingFlags.Public;
                }
            }
            else if (member is PropertyInfo property)
            {
                var method = property.GetMethod;

                if (method == null)
                {
                    return flags;
                }

                if (method.IsStatic)
                {
                    flags |= BindingFlags.Static;
                }
                else
                {
                    flags |= BindingFlags.Instance;
                }

                if (method.IsPublic == false)
                {
                    flags |= BindingFlags.NonPublic;
                }
                else
                {
                    flags |= BindingFlags.Public;
                }
            }

            return flags;
        }
        private static Filter MakeRange(string propertyName, object? value, RangeFilterType type)
        {
            if (value is null)
            {
                //TODO: FIX IT
                //return new LtFilter<object>(propertyName, null);
            }
            switch (value)
            {
                case string[] str: return new RangeFilter<string>(propertyName, str, type);
                case int[] int32: return new RangeFilter<int>(propertyName, int32, type);
                case long[] int64: return new RangeFilter<long>(propertyName, int64, type);
                case double[] doubleValue: return new RangeFilter<double>(propertyName, doubleValue, type);
                case decimal[] decimalValue: return new RangeFilter<decimal>(propertyName, decimalValue, type);
                case BsonObjectId[] objectId: return new RangeFilter<BsonObjectId>(propertyName, objectId, type);
                case BsonTimestamp[] timestamp: return new RangeFilter<BsonTimestamp>(propertyName, timestamp, type);
                case DateTimeOffset[] dt: return new RangeFilter<DateTimeOffset>(propertyName, dt, type);
                case Guid[] guid: return new RangeFilter<Guid>(propertyName, guid, type);
                case BsonDocument[] document: return new RangeFilter<BsonDocument>(propertyName, document, type);
            }

            return ThrowHelper.Expression<Filter>($"Unsupported type in Expression - {value.GetType()}");
        }
        private static Filter MakeEqual(string propertyName, object? value)
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
                case BsonDocument document: return Filter.FromDocument(document);
            }

            return ThrowHelper.Expression<Filter>($"Unsupported type in Expression(equal filter) - {value.GetType()}");
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

            return ThrowHelper.Expression<Filter>($"Unsupported type in Expression - {value.GetType()}");
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
        public static string? GetPropertyName<TIn, TOut>(Expression<Func<TIn, TOut>> expr) where TIn : IBsonSerializer<TIn>
        {
            var body = expr.Body;

            if (body is not MemberExpression memberExpr)
            {
                return null;
            }

            return memberExpr.Member.Name;
        }
    }
}
