using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly IdentifierNameSyntax DefaultBsonReaderId = SF.IdentifierName("reader");
        public static ExpressionSyntax TryGetDouble(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetDouble"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBoolean(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetBoolean"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryParseDocument(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryParseDocument"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetDateTimeWithBsonType(ExpressionSyntax typeId, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetDateTimeWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetGuidWithBsonType(ExpressionSyntax typeId, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetGuidWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt32(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt32"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt32(SyntaxToken target, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt32"), OutArgument(IdentifierName(target)));
        }
        public static ExpressionSyntax TryGetInt64(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt64"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt64(SyntaxToken target, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetInt64"), OutArgument(IdentifierName(target)));
        }
        public static ExpressionSyntax TryGetObjectId(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetObjectId"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetString(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetString"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetStringAsSpan(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetStringAsSpan"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetCStringAsSpan(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetCStringAsSpan"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetByte(ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryGetByte"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGeneric(SyntaxToken bsonType, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TryReadGeneric"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGenericNullable(TypeSyntax typeParam, SyntaxToken bsonType, ExpressionSyntax assignOrDecl, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, GenericName(Identifier("TryReadGeneric"), typeParam), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TrySkipCString(IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TrySkipCString"));
        }
        public static ExpressionSyntax TrySkip(ExpressionSyntax bsonType, IdentifierNameSyntax readerId = default)
        {
            return InvocationExpr(readerId ?? DefaultBsonReaderId, SF.IdentifierName("TrySkip"), SF.Argument(bsonType));
        }
        public static MemberAccessExpressionSyntax ReaderRemaining(IdentifierNameSyntax readerId = default)
        {
            return SimpleMemberAccess(readerId ?? DefaultBsonReaderId, SF.IdentifierName("Remaining"));
        }
    }
}