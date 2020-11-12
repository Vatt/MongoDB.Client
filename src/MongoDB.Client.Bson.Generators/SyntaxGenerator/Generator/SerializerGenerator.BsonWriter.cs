using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly IdentifierNameSyntax DefaultBsonWriterId = SF.IdentifierName("writer");
        public static InvocationExpressionSyntax Write_Type_Name_Value(IdentifierNameSyntax name, IdentifierNameSyntax value, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name_Value"), SF.Argument(name), SF.Argument(value));
        }
        public static InvocationExpressionSyntax Write_Type_Name(int typeid,  IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("Write_Type_Name"), SF.Argument(NumerictLiteralExpr(typeid)) , SF.Argument(name));
        }
        public static InvocationExpressionSyntax WriteBsonNull(IdentifierNameSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteBsonNull"), SF.Argument(name));
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
        public static InvocationExpressionSyntax WriteCString(ExpressionSyntax name, IdentifierNameSyntax writerId = default)
        {
            return InvocationExpr(writerId ?? DefaultBsonWriterId, SF.IdentifierName("WriteCString"), SF.Argument(name));
        }
    }
}