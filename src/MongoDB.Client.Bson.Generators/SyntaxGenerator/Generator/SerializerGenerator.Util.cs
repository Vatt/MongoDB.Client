using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    public static class ImmutableListBuilderExt
    {
        public static ImmutableList<StatementSyntax> ToStatements<T>(this ImmutableList<T> source)
            where T : ExpressionSyntax
        {
            return source.Select(x => SF.ExpressionStatement(x)).ToImmutableList<StatementSyntax>();
        }
        public static void IfStatement(this ImmutableList<StatementSyntax>.Builder builder, ExpressionSyntax condition, StatementSyntax statement, BlockSyntax @else)
        {
            builder.Add(SerializerGenerator.IfStatement(condition, statement, @else));
        }
        public static void IfStatement(this ImmutableList<StatementSyntax>.Builder builder, ExpressionSyntax condition, StatementSyntax statement)
        {
            builder.Add(SerializerGenerator.IfStatement(condition, statement));
        }
        public static void IfNot(this ImmutableList<StatementSyntax>.Builder builder, ExpressionSyntax condition, params StatementSyntax[] statement)
        {
            builder.Add(SerializerGenerator.IfNot(condition, statement));
        }
        public static  void DefaultLocalDeclarationStatement(this ImmutableList<StatementSyntax>.Builder builder, TypeSyntax type, SyntaxToken variable)
        {
            builder.Add(SerializerGenerator.DefaultLocalDeclarationStatement(type, variable));
        }
    }
    internal static partial class SerializerGenerator
    {
        internal class StatementsBuilder
        {
            private List<StatementSyntax> _statements = new();
            public bool IsEmpty => _statements.Count > 0;
            public StatementSyntax[] Build()
            {
                return _statements.ToArray();
            }
            public void Statements(params ExpressionSyntax[] exprs)
            {
                foreach (var expr in exprs)
                {
                    _statements.Add(SerializerGenerator.Statement(expr));
                }
            }
            public void Statements(params StatementSyntax[] statements)
            {
                _statements.AddRange(statements);
            }
            public void Statements(StatementsBuilder other)
            {
                _statements.AddRange(other.Build());
            }
            public void Statements(StatementSyntax expr)
            {
                _statements.Add(expr);
            }
            public void IfStatement(ExpressionSyntax condition, StatementSyntax statement)
            {
                Statements(SF.IfStatement(condition, statement));
            }
            public void DefaultLocalDeclarationStatement(TypeSyntax type, SyntaxToken variable)
            {
                Statements(SerializerGenerator.DefaultLocalDeclarationStatement(type, variable));
            }
        }
    }
}
