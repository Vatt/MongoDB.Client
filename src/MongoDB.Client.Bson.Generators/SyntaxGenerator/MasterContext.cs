using System.Collections.Generic;
using static MongoDB.Client.Bson.Generators.BsonSerializerGenerator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ContextCore> Contexts;
        public MasterContext(HashSet<BsonSerializerNode> symbols, System.Threading.CancellationToken cancellationToken)
        {
            Contexts = new List<ContextCore>();
            foreach (var pair in symbols)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (node, symbol) = pair;
                Contexts.Add(new ContextCore(node, symbol));

            }
        }
    }
}
