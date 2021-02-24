using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(ExpressionSyntax destination, IdentifierNameSyntax value)
        {
            return InvocationExpr(SF.IdentifierName("BinaryPrimitives"), SF.IdentifierName("WriteInt32LittleEndian"), SF.Argument(destination), SF.Argument(value));
        }
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(SyntaxToken destination, SyntaxToken value)
        {
            return InvocationExpr(
                SF.IdentifierName("BinaryPrimitives"),
                SF.IdentifierName("WriteInt32LittleEndian"),
                SF.Argument(IdentifierName(destination)),
                SF.Argument(IdentifierName(value)));
        }
    }
}