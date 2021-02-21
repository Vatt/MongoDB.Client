using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        internal class StatementsBuilder
        {
            private List<StatementSyntax> _statements = new();
            public StatementSyntax[] Build()
            {
                return _statements.ToArray();
            }
            public void Add(ExpressionSyntax expr)
            {
                _statements.Add(Statement(expr));
            }
            public void Add(params ExpressionSyntax[] exprs)
            {
                foreach (var expr in exprs)
                {
                    _statements.Add(Statement(expr));
                }
            }
            public void Add(params StatementSyntax[] statements)
            {
                _statements.AddRange(statements);
            }
            public void Add(StatementsBuilder other)
            {
                _statements.AddRange(other.Build());
            }
            public void Add(StatementSyntax expr)
            {
                _statements.Add(expr);
            }
            public void IfStatement(ExpressionSyntax condition, StatementSyntax statement)
            {
                Add(SF.IfStatement(condition, statement));
            }
            public void IfStatement(ExpressionSyntax condition, StatementSyntax statement, BlockSyntax @else)
            {
                Add(SF.IfStatement(condition, statement, SF.ElseClause(@else)));
            }
            public void DefaultLocalDeclarationStatement(TypeSyntax type, SyntaxToken variable)
            {
                Add(SerializerGenerator.DefaultLocalDeclarationStatement(type, variable));
            }
            public void IfNot(ExpressionSyntax condition, params StatementSyntax[] statement)
            {
                Add(SerializerGenerator.IfNot(condition, statement));
            }
        }
    }
}
