using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static InvocationExpressionSyntax BinaryPrimitivesWriteInt32LittleEndian(ExpressionSyntax destination, IdentifierNameSyntax value)
        {
            return InvocationExpr(SF.IdentifierName("BinaryPrimitives"), SF.IdentifierName("WriteInt32LittleEndian"), SF.Argument(destination), SF.Argument(value));
        }
    }
}