using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ContextCore> Contexts;
        public MasterContext(List<(SyntaxNode, INamedTypeSymbol)> symbols, System.Threading.CancellationToken cancellationToken)
        {
            Contexts = new List<ContextCore>();
            foreach (var pair in symbols)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (node, symbol) = pair;
                if (node is RecordDeclarationSyntax)
                {
                    Contexts.Add(new RecordContext(this, node, symbol));
                }
                else
                {
                    Contexts.Add(new ClassContext(this, node, symbol));
                }

            }
        }
    }
}
