using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly MemberAccessExpressionSyntax WriterWrittenExpr = SimpleMemberAccess(BsonWriterToken, IdentifierName("Written"));
        public static readonly ExpressionSyntax WriterCommitExpr = InvocationExpr(BsonWriterToken, IdentifierName("Commit"));
        public static ExpressionSyntax Write_Type_Name_Value(ExpressionSyntax name, ExpressionSyntax value)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Write_Type_Name_Value"), Argument(name), Argument(value));
        }
        public static ExpressionSyntax Write_Type_Name_Value(SyntaxToken name, ExpressionSyntax value)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Write_Type_Name_Value"), Argument(IdentifierName(name)), Argument(value));
        }
        public static ExpressionSyntax Write_Type_Name_Value(ExpressionSyntax name, int binaryDataSubtype, ExpressionSyntax value)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Write_Type_Name_Value"), Argument(name), Argument(NumericLiteralExpr(binaryDataSubtype)), Argument(value));
        }
        public static ExpressionSyntax Write_Type_Name(int typeid, IdentifierNameSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Write_Type_Name"), Argument(NumericLiteralExpr(typeid)), Argument(name));
        }
        public static ExpressionSyntax Write_Type_Name(int typeid, SyntaxToken name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Write_Type_Name"), Argument(NumericLiteralExpr(typeid)), Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteBsonNull(ExpressionSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteBsonNull"), Argument(name));
        }
        public static ExpressionSyntax WriteBsonNull(SyntaxToken name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteBsonNull"), Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteInt32(ExpressionSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteInt32"), Argument(name));
        }
        public static ExpressionSyntax WriteInt64(ExpressionSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteInt64"), Argument(name));
        }
        public static ExpressionSyntax WriteString(ExpressionSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteString"), Argument(name));
        }
        public static ExpressionSyntax WriteString(SyntaxToken name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteString"), Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteCString(ExpressionSyntax name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteCString"), Argument(name));
        }
        public static ExpressionSyntax WriteCString(SyntaxToken name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteCString"), Argument(IdentifierName(name)));
        }
        public static ExpressionSyntax WriteGeneric(ExpressionSyntax name, ExpressionSyntax reserved)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteGeneric"), Argument(name), RefArgument(reserved));
        }
        public static ExpressionSyntax WriteObject(ExpressionSyntax name, ExpressionSyntax reserved)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteObject"), Argument(name), RefArgument(reserved));
        }
        public static ExpressionSyntax WriterReserve(int size)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("Reserve"), Argument(NumericLiteralExpr(size)));
        }
        public static ExpressionSyntax ReservedWrite(SyntaxToken reserved, SyntaxToken target)
        {
            return InvocationExpr(IdentifierName(reserved), IdentifierName("Write"), Argument(IdentifierName(target)));
        }
        public static ExpressionSyntax ReservedWriteByte(SyntaxToken reserved, SyntaxToken target)
        {
            return InvocationExpr(IdentifierName(reserved), IdentifierName("WriteByte"), Argument(IdentifierName(target)));
        }
        public static ExpressionSyntax WriteByte(byte value)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteByte"), Argument(NumericLiteralExpr(value)));
        }
        public static ExpressionSyntax WriteByte(ExpressionSyntax value)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteByte"), Argument(value));
        }
        public static ExpressionStatementSyntax WriteByteStatement(byte value)
        {
            return SyntaxFactory.ExpressionStatement(InvocationExpr(BsonWriterToken, IdentifierName("WriteByte"), Argument(NumericLiteralExpr(value))));
        }
        public static ExpressionStatementSyntax WriteByteStatement(ExpressionSyntax value)
        {
            return SyntaxFactory.ExpressionStatement(InvocationExpr(BsonWriterToken, IdentifierName("WriteByte"), Argument(value)));
        }

        public static ExpressionSyntax WriteName(SyntaxToken name)
        {
            return InvocationExpr(BsonWriterToken, IdentifierName("WriteName"), Argument(name));
        }
    }
}
