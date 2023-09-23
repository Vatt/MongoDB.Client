using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        private static readonly TypeSyntax SerializerEndMarkerExceptionSyntax = SF.ParseTypeName("MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException");
        public static ThrowExpressionSyntax SerializerEndMarkerException(ISymbol symbol, ExpressionSyntax endMarker)
        {
            return SF.ThrowExpression(ObjectCreation(SerializerEndMarkerExceptionSyntax, SF.Argument(NameOf(IdentifierName(symbol.ToString()))), SF.Argument(endMarker)));
        }

        private static readonly TypeSyntax SerializerNotFoundExceptionSyntax = SF.ParseTypeName("MongoDB.Client.Bson.Serialization.Exceptions.SerializerNotFoundException");
        public static ThrowExpressionSyntax SerializerNotFoundException(ISymbol symbol)
        {
            return SF.ThrowExpression(ObjectCreation(SerializerNotFoundExceptionSyntax, SF.Argument(NameOf(IdentifierName(symbol.ToString())))));
        }
    }
}
