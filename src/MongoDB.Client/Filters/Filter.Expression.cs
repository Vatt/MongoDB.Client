using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Expressions;
using System.Linq.Expressions;

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
                        
                        var right = Next(binExpr.Right, ref ctx);
                        var left = Next(binExpr.Left, ref ctx);

                        return AggregateFilter.And(right, left);
                    }
                case ExpressionType.OrElse:
                    {
                        var binExpr = (BinaryExpression)expr;

                        var right = Next(binExpr.Right, ref ctx);
                        var left = Next(binExpr.Left, ref ctx);

                        return AggregateFilter.Or(right, left);
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
                            return FromCallExpr(binExpr, ref ctx);
                        }
                        return MakeSimpleFilter(binExpr, ref ctx);
                    }
                case ExpressionType.Call:
                    return FromCallExpr((MethodCallExpression)expr, true, ref ctx);
            }

            return ThrowHelper.Expression<Filter>($"Not supported expression {expr}");
        }
        private static Filter FromCallExpr(BinaryExpression binExpr, ref Context ctx)
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
                type = binExpr.Right.ExtractValue();
            }
            else
            {
                callExpr = (MethodCallExpression)binExpr.Right;
                type = binExpr.Left.ExtractValue();
            }

            if (type is not bool)
            {
                ThrowHelper.Expression<Filter>($"Not supported type in {binExpr}");
            }

            var typed = (bool)type;

            return typed switch
            {
                true when nodeType is ExpressionType.Equal => FromCallExpr(callExpr, true, ref ctx),
                true when nodeType is ExpressionType.NotEqual => FromCallExpr(callExpr, false, ref ctx),
                false when nodeType is ExpressionType.Equal => FromCallExpr(callExpr, false, ref ctx),
                _ => FromCallExpr(callExpr, true, ref ctx)
            };
        }
        private static Filter FromCallExpr(MethodCallExpression callExpr, bool type, ref Context ctx)
        {
            var methodName = callExpr.Method.Name;

            if (methodName is "Contains")
            {
                return MakeRangeFilter(callExpr, type, ref ctx);
            }
            else if (methodName is "Any" or "All")
            {
                return MakeArrayFilter(callExpr, methodName, type, ref ctx);
            }
            else
            {
                return ThrowHelper.Expression<Filter>($"Not supported method name {methodName}");
            }
        }
        private static Filter MakeArrayFilter(MethodCallExpression callExpr, string methodName, bool type, ref Context ctx)
        {
            string? propertyName;
            int? size = null;
            List<LambdaExpression> expressions = new(1);

            switch (callExpr.Arguments.Count)
            {
                case 2://default All/Any x => x.Collection.Any(y => x < 5)
                    propertyName = callExpr.Arguments[0].GetPropertyName();
                    expressions.Add((LambdaExpression)callExpr.Arguments[1]);
                    size = 0;

                    break;
                case 3:// Extension All/Any x => x.Collection.Any(2, y => y > 3, y => y < 5)
                    propertyName = callExpr.Arguments[0].GetPropertyName();
                    size = (int)callExpr.Arguments[1].ExtractValue()!;
                    foreach(var expr in ((NewArrayExpression)callExpr.Arguments[2]).Expressions)
                    {
                        expressions.Add((LambdaExpression)expr);
                    }
                    
                    break;
                default:
                    return ThrowHelper.Expression<Filter>($"Can't create ArrayFilter from {callExpr}");
            }
            return null;
            //static Filter ProcessBody(Expression expr)
            //{

            //}
        }
        private static Filter MakeRangeFilter(MethodCallExpression callExpr, bool type, ref Context ctx)
        {
            object? value;
            string? property;
            if (callExpr.Arguments.Count is 2)//static method
            {
                value = callExpr.Arguments[0].ExtractValue();
                property = callExpr.Arguments[1].GetPropertyName();

                if (value is null || property is null)
                {
                    return ThrowHelper.Expression<Filter>($"Not supported expression {callExpr}");
                }
            }
            else // instance method
            {
                //Not range filter, its Equal filter with LIKE semantic
                if (callExpr.Method.DeclaringType == typeof(string))
                {
                    if (callExpr.Arguments[0] is MemberExpression memberExpr && memberExpr.Expression == ctx.Parameter)// StringVar.Contains("find expr")
                    {
                        property = callExpr.Arguments[0].GetPropertyName();
                        value = callExpr.Object.ExtractValue();

                        return Create(property, $"/{value}/");
                    }
                    else //"asd".Contains(x.Name)
                    {
                        value = callExpr.Object.ExtractValue();
                        property = callExpr.Arguments[0].GetPropertyName();

                        return Create(property, $"/{value}/");
                    }
                }
                else
                {
                    property = callExpr.Arguments[0].GetPropertyName();
                    value = callExpr.Object.ExtractValue();
                }
            }

            if (property is null)
            {
                return ThrowHelper.Expression<Filter>($"Can't get property name from {callExpr}");
            }

            return type ? Create(property, value, RangeFilterType.In) : Create(property, value, RangeFilterType.NotIn);
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
            else if (binExpr.Left is UnaryExpression leftUnary && leftUnary.Operand is MemberExpression operandMember && operandMember.Expression == parameter)
            {
                propertyExpr = binExpr.Left;
                valueExpr = binExpr.Right;
            }
            else
            {
                propertyExpr = binExpr.Right;
                valueExpr = binExpr.Left;
            }

            object? value = Helper.ExtractValue(valueExpr);

            var property = Helper.GetPropertyName(propertyExpr);

            if (property is null)
            {
                return ThrowHelper.Expression<Filter>($"Not supported expression {binExpr}");
            }

            if (mapping.TryGetValue(property, out var mappedProperty) is true)
            {
                property = mappedProperty;
            }

            return operation switch
            {
                ExpressionType.Equal => Create(property, value),
                ExpressionType.LessThan => Create(property, value, FilterType.Lt),
                ExpressionType.LessThanOrEqual => Create(property, value, FilterType.Lte),
                ExpressionType.GreaterThan => Create(property, value, FilterType.Gt),
                ExpressionType.GreaterThanOrEqual => Create(property, value, FilterType.Gte),
                ExpressionType.NotEqual => Create(property, value, FilterType.Ne),
                _ => throw new NotSupportedException($"Not supported  operation {operation}")
            };
        }
    }
}
