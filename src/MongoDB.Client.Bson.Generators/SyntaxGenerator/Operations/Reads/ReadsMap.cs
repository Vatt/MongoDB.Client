using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads;
using System.Collections.Generic;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal static class ReadsMap
    {
        internal static readonly Dictionary<string, ReadBase> SimpleOperations = new Dictionary<string, ReadBase>()
        {
            ["Double"] = new DoubleRead(GeneratorBasics.ReaderInputVariableIdentifier),
            ["String"] = new StringRead(GeneratorBasics.ReaderInputVariableIdentifier),
            //["BsonDocument"] = SF.IdentifierName("TryParseDocument"),
            ["BsonObjectId"] = new ObjectIdRead(GeneratorBasics.ReaderInputVariableIdentifier),
            ["Boolean"] = new BooleanRead(GeneratorBasics.ReaderInputVariableIdentifier),
            ["Int32"] = new Int32Read(GeneratorBasics.ReaderInputVariableIdentifier),
            ["Int64"] = new Int64Read(GeneratorBasics.ReaderInputVariableIdentifier),
            ["Guid"] = new GuidRead(GeneratorBasics.ReaderInputVariableIdentifier),
            ["DateTimeOffset"] = new DateTimeOffsetRead(GeneratorBasics.ReaderInputVariableIdentifier)

        };
        internal static bool TryGetValue(INamedTypeSymbol sym, out ReadBase readOp)
        {
            return SimpleOperations.TryGetValue(sym.Name, out readOp);
        }
    }
}
