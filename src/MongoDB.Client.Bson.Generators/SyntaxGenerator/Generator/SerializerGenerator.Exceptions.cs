using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ThrowExpressionSyntax SerializerEndMarkerException(ExpressionSyntax serializer, ExpressionSyntax endMarker)
        {
            
            var exception = SF.ParseTypeName("MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException");
            return SF.ThrowExpression(ObjectCreation(exception, SF.Argument(NameOf(serializer)), SF.Argument(endMarker)));
        }
    }
}