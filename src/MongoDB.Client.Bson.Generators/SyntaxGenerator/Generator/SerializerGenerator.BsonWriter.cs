using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly IdentifierNameSyntax DefaultBsonWriterId = SF.IdentifierName("writer");
        public static ExpressionSyntax Write_Type_Name_Value(ExpressionSyntax name, ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name_Value"), SF.Argument(name), SF.Argument(value));
        }
        public static ExpressionSyntax Write_Type_Name_Value(SyntaxToken name, ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name_Value"), SF.Argument(IdentifierName(name)), SF.Argument(value));
        }

        public static ExpressionSyntax Write_Type_Name(int typeid, IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name"), SF.Argument(NumericLiteralExpr(typeid)), SF.Argument(name));
        }
        public static ExpressionSyntax Write_Type_Name(int typeid, SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name"), SF.Argument(NumericLiteralExpr(typeid)), SF.Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteBsonNull(IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteBsonNull"), SF.Argument(name));
        }
        public static ExpressionSyntax WriteBsonNull(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteBsonNull"), SF.Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteInt32(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteInt32"), SF.Argument(name));
        }
        public static ExpressionSyntax WriteInt64(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteInt64"), SF.Argument(name));
        }
        public static ExpressionSyntax WriteString(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteString"), SF.Argument(name));
        }
        public static ExpressionSyntax WriteString(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteString"), SF.Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteCString(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteCString"), SF.Argument(name));
        }
        public static ExpressionSyntax WriteCString(SyntaxToken name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteCString"), SF.Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteGeneric(ExpressionSyntax name, ExpressionSyntax reserved, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteGeneric"), SF.Argument(name), RefArgument(reserved));
        }
        public static MemberAccessExpressionSyntax WriterWritten(IdentifierNameSyntax writerId = default)
        {
            return SimpleMemberAccess(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Written"));
        }
        public static ExpressionSyntax WriterReserve(int size, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Reserve"), SF.Argument(NumericLiteralExpr(size)));
        }
        public static ExpressionSyntax ReservedWrite(SyntaxToken reserved, SyntaxToken target)
        {
            return InvocationExpr(IdentifierName(reserved), IdentifierName("Write"), SF.Argument(IdentifierName(target)));
        }
        public static ExpressionSyntax WriteByte(byte value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(NumericLiteralExpr(value)));
        }
        public static ExpressionSyntax WriteByte(ExpressionSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteByte"), SF.Argument(value));
        }
        public static ExpressionSyntax WriterCommit(IdentifierNameSyntax writerId = default)
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