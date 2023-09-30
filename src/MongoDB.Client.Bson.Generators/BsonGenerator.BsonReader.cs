using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        public static MemberAccessExpressionSyntax ReaderRemainingExpr => SimpleMemberAccess(BsonReaderToken, IdentifierName("Remaining"));
        public static ExpressionSyntax TrySkipCStringExpr => InvocationExpr(BsonReaderToken, SF.IdentifierName("TrySkipCString"));
        public static ExpressionSyntax TryGet(ExpressionSyntax bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, IdentifierName("TryGet"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetDouble(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, IdentifierName("TryGetDouble"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetDecimal(ExpressionSyntax bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, IdentifierName("TryGetDecimal"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBoolean(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetBoolean"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryParseDocument(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryParseDocument"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetUtcDatetime(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetUtcDatetime"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBinaryDataGuid(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetBinaryDataGuid"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt32(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt32"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetInt64(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetInt64"), OutArgument(assignOrDecl));
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
        public static ExpressionSyntax TryGetCString(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetCString"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetByte(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetByte"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBsonType(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetBsonType"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGeneric(SyntaxToken bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryReadGeneric"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadGenericNullable(SyntaxToken bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(IdentifierName(BsonReaderToken), Identifier("TryReadGenericNullable"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryReadObject(ExpressionSyntax bsonType, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryReadObject"), Argument(bsonType), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TrySkip(SyntaxToken bsonType)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TrySkip"), SF.Argument(IdentifierName(bsonType)));
        }
        public static ExpressionSyntax TryGetTimestamp(ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, SF.IdentifierName("TryGetTimestamp"), OutArgument(assignOrDecl));
        }
        public static ExpressionSyntax TryGetBinaryData(int subtype, ExpressionSyntax assignOrDecl)
        {
            return InvocationExpr(BsonReaderToken, IdentifierName("TryGetBinaryData"), Argument(NumericLiteralExpr(subtype)), OutArgument(assignOrDecl));
        }
    }
}
