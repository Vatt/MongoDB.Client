using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{

    internal class ContextCore
    {
        internal MasterContext Root { get; }
        internal INamedTypeSymbol Declaration { get; }
        internal SyntaxNode DeclarationNode { get; }
        internal List<MemberContext> Members { get; }
        internal ImmutableArray<ITypeSymbol>? GenericArgs { get; }
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
        public ContextCore(MasterContext root, SyntaxNode node, INamedTypeSymbol symbol)
        {
            Root = root;
            Declaration = symbol;
            DeclarationNode = node;
            Members = new List<MemberContext>();
            GenericArgs = Declaration.TypeArguments.IsEmpty ? null : Declaration.TypeArguments;
            if (SerializerGenerator.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }

            foreach (var member in Declaration.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract ||
                    SerializerGenerator.IsIgnore(member) || (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field) ||
                    member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }
       
                //TODO: допустимо только без гетера, если нет сетера проверить есть ли он в конструкторе
                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false}) ||
                     (member is IFieldSymbol { IsReadOnly: false, IsConst: false}))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
                if(member is IPropertySymbol prop && (prop.IsReadOnly || prop.GetMethod != null) && ConstructorContains(member.Name))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
                if (member is IFieldSymbol field && field.IsReadOnly && field.IsConst == false && ConstructorContains(member.Name))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
            }
        }
        public bool ConstructorContains(string name)
        {
            if (ConstructorParams.HasValue)
            {
                var param = ConstructorParams.Value.FirstOrDefault(type => type.Name.Equals(name));
                return param != null;
            }

            return false;
        }
    }
}