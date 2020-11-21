using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly IdentifierNameSyntax DefaultBsonWriterId = SF.IdentifierName("writer");
        public static InvocationExpressionSyntax Write_Type_Name_Value(ExpressionSyntax name, ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name_Value"), SF.Argument(name), SF.Argument(value));
        }
        public static InvocationExpressionSyntax Write_Type_Name(int typeid, IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name"), SF.Argument(NumericLiteralExpr(typeid)), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteBsonNull(IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteBsonNull"), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteBsonNull(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteBsonNull"), SF.Argument(IdentifierName(name)));
        }
        public static InvocationExpressionSyntax WriteInt32(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteInt32"), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteInt64(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteInt64"), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteString(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteString"), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteString(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteString"), SF.Argument(IdentifierName(name)));
        }
        public static InvocationExpressionSyntax WriteCString(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteCString"), SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteCString(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteCString"), SF.Argument(IdentifierName(name)));
        }
        public static InvocationExpressionSyntax WriteGeneric(ExpressionSyntax name, ExpressionSyntax reserved, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteGeneric"), SF.Argument(name), RefArgument(reserved));
        }
        public static MemberAccessExpressionSyntax WriterWritten(IdentifierNameSyntax writerId = default)
        {
            return SimpleMemberAccess(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Written"));
        }
        public static InvocationExpressionSyntax WriterReserve(int size, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Reserve"), SF.Argument(NumericLiteralExpr(size)));
        }
        public static InvocationExpressionSyntax ReservedWrite(SyntaxToken reserved, SyntaxToken target)
        {
            return InvocationExpr(IdentifierName(reserved), IdentifierName("Write"), SF.Argument(IdentifierName(target)));
        }
        public static InvocationExpressionSyntax WriteByte(byte value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(NumericLiteralExpr(value)));
        }
        public static InvocationExpressionSyntax WriteByte(ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(value));
        }
        public static InvocationExpressionSyntax WriterCommit(IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Commit"));
        }
        public static ExpressionStatementSyntax WriteByteStatement(byte value, IdentifierNameSyntax writerId = default)
        {
            return SF.ExpressionStatement(InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(NumericLiteralExpr(value))));
        }
        public static ExpressionStatementSyntax WriteByteStatement(ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return SF.ExpressionStatement(InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(value)));
        }
    }
}