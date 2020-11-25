using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ContextCore> Contexts;
        public readonly Compilation Compilation;
        public MasterContext(List<(SyntaxNode, INamedTypeSymbol)> symbols, GeneratorExecutionContext ctx)
        {
            Contexts = new List<ContextCore>();
            Compilation = ctx.Compilation;
            var typelib = TypeLib.FromCompilation(Compilation);
            foreach (var pair in symbols)
            {
                var (node, symbol) = pair;
                if (node is RecordDeclarationSyntax)
                {
                    Contexts.Add(new RecordContext(this, typelib, node, symbol));
                }
                else
                {
                    Contexts.Add(new ClassContext(this, typelib, node, symbol));
                }
                
            }
        }
    }
}
