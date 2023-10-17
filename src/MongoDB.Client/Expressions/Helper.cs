using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Filters;

namespace MongoDB.Client.Expressions
{
    internal static class Helper
    {
        internal static object? ExtractValue(this Expression expr) => expr switch
        {
            ConstantExpression constExpr => ExtractValue(constExpr),
            MemberExpression memberExpr => ExtractValue(memberExpr),
            _ => ThrowHelper.Expression<Filter>($"Can't extract value from expression {expr}")
        };
        internal static object? ExtractValue(this ConstantExpression constExpr)
        {
            return constExpr.Value;
        }
        internal static object? ExtractValue(this MemberExpression memberExpr)
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
                var flags = Helper.GetBindingFlags(memberExpr.Member);
                var field = constExpr.Value!.GetType().GetField(closureName, flags);
                return field!.GetValue(constExpr.Value);
            }
            else if (memberExpr.Expression is MemberExpression innerExpr) //Member1.Member2.Member3.Value
            {
                List<(string, MemberTypes, BindingFlags)> trace = new();
                while (innerExpr.Expression is not null)
                {
                    trace.Add((innerExpr.Member.Name, innerExpr.Member.MemberType, Helper.GetBindingFlags(innerExpr.Member)));

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
        internal static BindingFlags GetBindingFlags(this MemberInfo member)
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
        internal static string? GetPropertyName(this Expression expr)
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
        internal static string? GetPropertyName<TIn, TOut>(Expression<Func<TIn, TOut>> expr) where TIn : IBsonSerializer<TIn>
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
