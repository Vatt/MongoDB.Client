using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal static class ReadOpsMethodIdentifiers
    {
        internal static readonly Dictionary<string, IdentifierNameSyntax> SimpleOperations = new Dictionary<string, IdentifierNameSyntax>()
        {
            ["Double"] = SF.IdentifierName("TryGetDouble"),
            ["String"] = SF.IdentifierName("TryGetString"),
            ["BsonDocument"] = SF.IdentifierName("TryParseDocument"),
            ["BsonObjectId"] = SF.IdentifierName("TryGetObjectId"),
            ["Boolean"] = SF.IdentifierName("TryGetBoolean"),
            ["Int32"] = SF.IdentifierName("TryGetInt32"),
            ["Int64"] = SF.IdentifierName("TryGetInt64"),
            ["Guid"] = SF.IdentifierName("TryGetGuidWithBsonType"),
            ["DateTimeOffset"] = SF.IdentifierName("TryGetDateTimeWithBsonType")

        };
        internal static bool TryGetValue(INamedTypeSymbol sym, out IdentifierNameSyntax identifier)
        {
            return SimpleOperations.TryGetValue(sym.Name, out identifier);
        }
    }
}
