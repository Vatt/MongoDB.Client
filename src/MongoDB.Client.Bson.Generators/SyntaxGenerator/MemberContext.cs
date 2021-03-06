using System;
using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Immutable;
using System.Text;

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
        internal readonly SyntaxToken StaticSpanNameToken;
        internal readonly SyntaxToken AssignedVariableToken;
        internal readonly Memory<byte> ByteName;
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
            StaticSpanNameToken = SerializerGenerator.Identifier($"{Root.Declaration.Name}{BsonElementAlias}");
            var trueType = SerializerGenerator.ExtractTypeFromNullableIfNeed(TypeSym);
            if (trueType.IsReferenceType)
            {
                AssignedVariableToken = SerializerGenerator.Identifier($"{trueType.Name}{NameSym.Name}");
            }
            else
            {
                AssignedVariableToken = SerializerGenerator.Identifier($"{TypeSym.Name}{NameSym.Name}");
            }
            ByteName = Encoding.UTF8.GetBytes(BsonElementValue);
        }

    }
}
