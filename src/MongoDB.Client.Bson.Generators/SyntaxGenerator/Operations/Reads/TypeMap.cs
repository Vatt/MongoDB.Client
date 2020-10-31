using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads;
using System.Collections.Generic;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal static class TypeMap
    {
        internal static readonly Dictionary<string, ReadWriteBase> SimpleOperations = new Dictionary<string, ReadWriteBase>()
        {
            ["Double"] = new DoubleRead(Basics.ReaderInputVariableIdentifier),
            ["String"] = new StringRead(Basics.ReaderInputVariableIdentifier),
            ["BsonDocument"] = new BsonDocumentRead(Basics.ReaderInputVariableIdentifier),
            ["BsonObjectId"] = new ObjectIdRead(Basics.ReaderInputVariableIdentifier),
            ["Boolean"] = new BooleanRead(Basics.ReaderInputVariableIdentifier),
            ["Int32"] = new Int32Read(Basics.ReaderInputVariableIdentifier),
            ["Int64"] = new Int64Read(Basics.ReaderInputVariableIdentifier),
            ["Guid"] = new GuidRead(Basics.ReaderInputVariableIdentifier),
            ["DateTimeOffset"] = new DateTimeOffsetRead(Basics.ReaderInputVariableIdentifier)

        };
        internal static bool TryGetValue(ITypeSymbol sym, out ReadWriteBase readOp)
        {
            return SimpleOperations.TryGetValue(sym.Name, out readOp);
        }
    }
}
