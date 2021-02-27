using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class ClassContext : ContextCore
    {
        public ClassContext(MasterContext root, SyntaxNode node, INamedTypeSymbol symbol) : base(root, node, symbol)
        {

        }
    }
}

