using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    public static class CompilationExt
    {
        internal static IEnumerable<INamedTypeSymbol> GetTypesByMetadataName(this Compilation compilation, string typeMetadataName)
        {
            return compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(typeMetadataName))
                .Where(t => t != null);
        }
    }
}