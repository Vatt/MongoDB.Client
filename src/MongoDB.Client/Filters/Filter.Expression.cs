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
                true when nodeType is ExpressionType.Equal => FromCallExpr(callExpr, true, ref ctx),
                true when nodeType is ExpressionType.NotEqual => FromCallExpr(callExpr, false, ref ctx),
                false when nodeType is ExpressionType.Equal => FromCallExpr(callExpr, false, ref ctx),
                _ => FromCallExpr(callExpr, true, ref ctx)
            };
        }
        private static Filter FromCallExpr(MethodCallExpression callExpr, bool type, ref Context ctx)
        {
            var methodName = callExpr.Method.Name;

            if (callExpr.Method.Name != "Contains")
            {
                return ThrowHelper.Expression<Filter>($"Not supported MethodCallExpression with method name {methodName}");
            }
            object? value;
            string? property;
            if (callExpr.Arguments.Count is 2)//static method
            {
                value = ExtractValue(callExpr.Arguments[0]);
                property = GetPropertyName(callExpr.Arguments[1]);

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
                        property = GetPropertyName(callExpr.Arguments[0]);
                        value = ExtractValue(callExpr.Object);

                        return Create(property, $"/{value}/");
                    }
                    else //"asd".Contains(x.Name)
                    {
                        value = ExtractValue(callExpr.Object);
                        property = GetPropertyName(callExpr.Arguments[0]);

                        return Create(property, $"/{value}/");
                    }
                }
                else
                {
                    property = GetPropertyName(callExpr.Arguments[0]);
                    value = ExtractValue(callExpr.Object);
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

            object? value = ExtractValue(valueExpr);

            var property = GetPropertyName(propertyExpr);

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
            
            if (memberExpr.Expression is null) //static variable
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
            else if (memberExpr.Expression!.NodeType is ExpressionType.Constant) // simple constant value
            {
                constExpr = (ConstantExpression)memberExpr.Expression;
                var flags = GetBindingFlags(memberExpr.Member);
                var field = constExpr.Value!.GetType().GetField(closureName, flags);
                return field!.GetValue(constExpr.Value);
            }
            else if (memberExpr.Expression is MemberExpression innerExpr) //Member1.Member2.Member3.Value
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
                        MemberTypes.Field => value!.GetType().GetField(field, flags)!.GetValue(value)!,
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
        public static string? GetPropertyName(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression memberExpr:
                    return memberExpr.Member.Name;
                case UnaryExpression unaryExpr:
                    return unaryExpr.Operand is MemberExpression operandMember ? operandMember.Member.Name : null;
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
