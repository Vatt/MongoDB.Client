using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{

    internal class ContextCore
    {
        internal MasterContext Root;
        internal INamedTypeSymbol Declaration;
        internal SyntaxNode DeclarationNode;
        internal List<MemberContext> Members;
        internal ImmutableArray<ITypeSymbol>? GenericArgs;
        internal ImmutableArray<IParameterSymbol>? ConstructorParams;
        internal SyntaxToken SerializerName
        {
            get
            {
                string generics = GenericArgs.HasValue && GenericArgs.Value.Length > 0
                    ? string.Join(string.Empty, GenericArgs.Value)
                    : string.Empty;
                return SerializerGenerator.Identifier($"{Declaration.ContainingNamespace.ToString().Replace(".", string.Empty)}{Declaration.Name}{generics}SerializerGenerated");
            }
        }
        internal bool HavePrimaryConstructor => ConstructorParams.HasValue;

        public bool ConstructorContains(string name)
        {
            if (ConstructorParams.HasValue)
            {
                _ = ConstructorParams.Value.First(type => type.Name.Equals(name));
                return true;
            }

            return false;
        }
    }
}