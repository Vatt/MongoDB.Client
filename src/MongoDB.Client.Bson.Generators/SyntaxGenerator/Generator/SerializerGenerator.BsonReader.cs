﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly IdentifierNameSyntax DefaultBsonReaderId = SF.IdentifierName("reader");
        public static InvocationExpressionSyntax TryGetDouble(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetDouble"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetBoolean(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetBoolean"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryParseDocument(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryParseDocument"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetDateTimeWithBsonType(IdentifierNameSyntax typeId, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetDateTimeWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetGuidWithBsonType(IdentifierNameSyntax typeId, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetGuidWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetInt32(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt32"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetInt64(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt64"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetObjectId(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetObjectId"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetString(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetString"), OutArgument(assignOrDecl));
        }
        public static InvocationExpressionSyntax TryGetStringAsSpan(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetStringAsSpan"), OutArgument(assignOrDecl));
        }
    }
}