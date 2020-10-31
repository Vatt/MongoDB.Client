using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using System.Collections.Generic;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite
{
    internal static class TypeMap
    {
        internal static readonly Dictionary<string, ReadWriteBase> SimpleOperations = new Dictionary<string, ReadWriteBase>()
        {
            ["Double"] = new DoubleRW(Basics.ReaderInputVariableIdentifier),
            ["String"] = new StringRW(Basics.ReaderInputVariableIdentifier),
            ["BsonDocument"] = new BsonDocumentRW(Basics.ReaderInputVariableIdentifier),
            ["BsonObjectId"] = new ObjectIdRW(Basics.ReaderInputVariableIdentifier),
            ["Boolean"] = new BooleanRW(Basics.ReaderInputVariableIdentifier),
            ["Int32"] = new Int32RW(Basics.ReaderInputVariableIdentifier),
            ["Int64"] = new Int64RW(Basics.ReaderInputVariableIdentifier),
            ["Guid"] = new GuidRW(Basics.ReaderInputVariableIdentifier),
            ["DateTimeOffset"] = new DateTimeOffsetRW(Basics.ReaderInputVariableIdentifier)

        };
        internal static bool TryGetValue(ITypeSymbol sym, out ReadWriteBase readOp)
        {
            return SimpleOperations.TryGetValue(sym.Name, out readOp);
        }
    }
}
