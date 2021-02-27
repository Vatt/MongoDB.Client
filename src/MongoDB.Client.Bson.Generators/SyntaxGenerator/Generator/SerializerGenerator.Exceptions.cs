using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ThrowExpressionSyntax SerializerEndMarkerException(ISymbol symbol, ExpressionSyntax endMarker)
        {
            var exception = SF.ParseTypeName("MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException");
            return SF.ThrowExpression(ObjectCreation(exception, SF.Argument(NameOf(IdentifierName(symbol.ToString()))), SF.Argument(endMarker)));
        }
        public static ThrowExpressionSyntax SerializerNotFoundException(ISymbol symbol)
        {

            var exception = SF.ParseTypeName("MongoDB.Client.Bson.Serialization.Exceptions.SerializerNotFoundException");
            return SF.ThrowExpression(ObjectCreation(exception, SF.Argument(NameOf(IdentifierName(symbol.ToString())))));
        }
    }
}