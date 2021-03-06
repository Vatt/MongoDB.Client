using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly MemberAccessExpressionSyntax ReaderRemainingExpr = SimpleMemberAccess(BsonReaderToken, IdentifierName("Remaining"));
        public static readonly ExpressionSyntax TrySkipCStringExpr = InvocationExpr(BsonReaderToken, SF.IdentifierName("TrySkipCString"));
        public static ExpressionSyntax TryGetDouble(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, IdentifierName("TryGetDouble"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBoolean(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetBoolean"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryParseDocument(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryParseDocument"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetDateTimeWithBsonType(ExpressionSyntax typeId, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetDateTimeWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetGuidWithBsonType(ExpressionSyntax typeId, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetGuidWithBsonType"), SF.Argument(typeId), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt32(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt32"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt32(SyntaxToken target)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt32"), OutArgument(IdentifierName(target)));
        }
        public static ExpressionSyntax TryGetInt64(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt64"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt64(SyntaxToken target)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt64"), OutArgument(IdentifierName(target)));
        }
        public static ExpressionSyntax TryGetObjectId(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetObjectId"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetString(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetString"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetStringAsSpan(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetStringAsSpan"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetCStringAsSpan(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetCStringAsSpan"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetByte(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetByte"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGeneric(SyntaxToken bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryReadGeneric"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGenericNullable(TypeSyntax typeParam, SyntaxToken bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(IdentifierName(BsonReaderToken), GenericName(Identifier("TryReadGeneric"), typeParam), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TrySkip(ExpressionSyntax bsonType)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TrySkip"), SF.Argument(bsonType));
        }
        public static ExpressionSyntax TryGetTimestamp(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetTimestamp"), OutArgument(assignOrDecl));
        }
    }
}