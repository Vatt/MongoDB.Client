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
            ["Double"] = new DoubleRead(Basics.ReaderInputVariableIdentifier),
            ["String"] = new StringRead(Basics.ReaderInputVariableIdentifier),
            //["BsonDocument"] = SF.IdentifierName("TryParseDocument"),
            ["BsonObjectId"] = new ObjectIdRead(Basics.ReaderInputVariableIdentifier),
            ["Boolean"] = new BooleanRead(Basics.ReaderInputVariableIdentifier),
            ["Int32"] = new Int32Read(Basics.ReaderInputVariableIdentifier),
            ["Int64"] = new Int64Read(Basics.ReaderInputVariableIdentifier),
            ["Guid"] = new GuidRead(Basics.ReaderInputVariableIdentifier),
            ["DateTimeOffset"] = new DateTimeOffsetRead(Basics.ReaderInputVariableIdentifier)

        };
        internal static bool TryGetValue(ITypeSymbol sym, out ReadBase readOp)
        {
            return SimpleOperations.TryGetValue(sym.Name, out readOp);
        }
    }
}
