using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{

    internal class ContextCore
    {


        internal bool IsRecord;
        internal MasterContext Root;
        internal INamedTypeSymbol Declaration;
        internal SyntaxNode DeclarationNode;
        internal List<MemberContext> Members;
        internal ImmutableArray<ITypeSymbol>? GenericArgs;
        internal ImmutableArray<IParameterSymbol>? ConstructorParams;

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