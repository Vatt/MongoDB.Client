using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(ExpressionSyntax destination, IdentifierNameSyntax value)
        {
            return InvocationExpr(IdentifierName("BinaryPrimitives"), IdentifierName("WriteInt32LittleEndian"), Argument(destination), Argument(value));
        }
        public static ExpressionSyntax BinaryPrimitivesReadInt32LittleEndian(SyntaxToken spanToken)
        {
            return InvocationExpr(IdentifierName("BinaryPrimitives"), IdentifierName("ReadInt32LittleEndian"), Argument(spanToken));
        }
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(SyntaxToken destination, SyntaxToken value)
        {
            return InvocationExpr(IdentifierName("BinaryPrimitives"),
                                  IdentifierName("WriteInt32LittleEndian"),
                                  Argument(IdentifierName(destination)),
                                  Argument(IdentifierName(value)));
        }
    }
}
