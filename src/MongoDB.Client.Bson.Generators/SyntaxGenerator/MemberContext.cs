using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Immutable;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MemberContext
    {
        internal ContextCore Root { get; }
        internal readonly ISymbol NameSym;
        internal readonly ITypeSymbol TypeSym;
        internal readonly string BsonElementAlias;
        internal readonly string BsonElementValue;
        internal readonly ImmutableArray<ITypeSymbol>? TypeGenericArgs;
        internal SyntaxToken AssignedVariable;
        public MemberContext(ContextCore root, ISymbol memberSym)
        {
            Root = root;
            NameSym = memberSym;

            switch (NameSym)
            {
                case IFieldSymbol field:
                    TypeSym = field.Type;
                    break;
                case IPropertySymbol prop:
                    TypeSym = prop.Type;
                    break;
                default: break;
            }

            if (TypeSym is INamedTypeSymbol namedType)
            {
                TypeGenericArgs = namedType.TypeArguments.IsEmpty ? null : namedType.TypeArguments;
            }

            (BsonElementValue, BsonElementAlias) = SerializerGenerator.GetMemberAlias(NameSym);
        }

    }
}
