using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MemberContext
    {
        internal ContextCore Root { get; }
        internal ISymbol NameSym { get; }
        internal ITypeSymbol TypeSym { get; }
        internal string BsonElementAlias { get; }
        internal string BsonElementValue { get; }
        internal ImmutableArray<ITypeSymbol>? TypeGenericArgs { get; }
        internal SyntaxToken StaticSpanNameToken { get; }
        internal SyntaxToken AssignedVariableToken { get; }
        internal Memory<byte> ByteName { get; }
        internal bool IsReadOnly { get; }
        public MemberContext(ContextCore root, ISymbol memberSym)
        {
            Root = root;
            NameSym = memberSym;

            switch (NameSym)
            {
                case IFieldSymbol field:
                    TypeSym = field.Type;
                    IsReadOnly = field.IsReadOnly;
                    break;
                case IPropertySymbol prop:
                    TypeSym = prop.Type;
                    IsReadOnly = prop.IsReadOnly;
                    break;
                default: break;
            }

            if (TypeSym is INamedTypeSymbol namedType)
            {
                TypeGenericArgs = namedType.TypeArguments.IsEmpty ? null : namedType.TypeArguments;
            }

            (BsonElementValue, BsonElementAlias) = SerializerGenerator.GetMemberAlias(NameSym);

            StaticSpanNameToken = SerializerGenerator.Identifier($"{Root.Declaration.Name}{BsonElementAlias}");

            var trueType = SerializerGenerator.ExtractTypeFromNullableIfNeed(TypeSym);
            
            if (trueType.IsReferenceType)
            {
                if (trueType.Equals(SerializerGenerator.ArrayByteTypeSym, SymbolEqualityComparer.Default))
                {
                    AssignedVariableToken = SerializerGenerator.Identifier($"ByteArray{NameSym.Name}");
                }
                else
                {
                    AssignedVariableToken = SerializerGenerator.Identifier($"{trueType.Name}{NameSym.Name}");
                }

            }
            else
            {
                AssignedVariableToken = SerializerGenerator.Identifier($"{TypeSym.Name}{NameSym.Name}");
            }

            ByteName = Encoding.UTF8.GetBytes(BsonElementValue);
        }
    }
}
