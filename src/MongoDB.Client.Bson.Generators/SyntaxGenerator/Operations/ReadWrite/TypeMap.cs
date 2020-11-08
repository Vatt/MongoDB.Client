using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using System.Collections.Generic;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite
{
    internal static class TypeMap
    {
        internal static readonly Dictionary<string, ReadWriteBase> BaseOperations = new Dictionary<string, ReadWriteBase>()
        {
            ["Double"] = new DoubleRW(),
            ["String"] = new StringRW(),
            ["BsonDocument"] = new BsonDocumentRW(),
            ["BsonObjectId"] = new ObjectIdRW(),
            ["Boolean"] = new BooleanRW(),
            ["Int32"] = new Int32RW(),
            ["Int64"] = new Int64RW(),
            ["Guid"] = new GuidRW(),
            ["DateTimeOffset"] = new DateTimeOffsetRW()

        };
        internal static bool TryGetValue(ITypeSymbol sym, out ReadWriteBase rwOp)
        {
            return BaseOperations.TryGetValue(sym.Name, out rwOp);
        }
        internal static bool TryGetValue(string typeName, out ReadWriteBase rwOp)
        {
            return BaseOperations.TryGetValue(typeName, out rwOp);
        }
    }
}
