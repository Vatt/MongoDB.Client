using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class ClassContext : ContextCore
    {
        public ClassContext(MasterContext root, SyntaxNode node, INamedTypeSymbol symbol)
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
                    SerializerGenerator.IsIgnore(member) || (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field))
                {
                    continue;
                }
                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    if (ConstructorParams == null)
                    {
                        continue;
                    }

                    foreach (var param in ConstructorParams)
                    {
                        if (param.Name.Equals(member.Name))
                        {
                            Members.Add(new MemberContext(this, member));
                            break;
                        }
                    }
                    continue;
                }
                //TODO: допустимо только без гетера, если нет сетера проверить есть ли он в конструкторе
                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                     (member is IFieldSymbol { IsReadOnly: false }))
                {
                    Members.Add(new MemberContext(this, member));
                }
            }
        }
    }
}

