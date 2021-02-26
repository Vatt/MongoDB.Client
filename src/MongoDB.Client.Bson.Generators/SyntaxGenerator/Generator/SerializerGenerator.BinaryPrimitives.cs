using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(ExpressionSyntax destination, IdentifierNameSyntax value)
        {
            return InvocationExpr(IdentifierName("BinaryPrimitives"), IdentifierName("WriteInt32LittleEndian"), Argument(destination), Argument(value));
        }
        public static ExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(SyntaxToken destination, SyntaxToken value)
        {
            return InvocationExpr(
                IdentifierName("BinaryPrimitives"),
                IdentifierName("WriteInt32LittleEndian"),
                Argument(IdentifierName(destination)),
                Argument(IdentifierName(value)));
        }
    }
}