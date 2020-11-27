using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class RecordContext : ContextCore
    {
        public RecordContext(MasterContext root, SyntaxNode node, INamedTypeSymbol symbol)
        {
            Root = root;
            Declaration = symbol;
            DeclarationNode = node;
            Members = new List<MemberContext>();
            IsRecord = true;
            GenericArgs = Declaration.TypeArguments.IsEmpty ? null : Declaration.TypeArguments;
            if (AttributeHelper.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }
            if (Declaration.Constructors.Length == 2)
            {
                ConstructorParams = Declaration.Constructors.Where(x => x.Parameters[0].Type != Declaration).First().Parameters;
            }
            var mem = symbol as IFieldSymbol;
            ProcessRecordInherit(symbol.BaseType);
            foreach (var member in Declaration.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
                     (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field))
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
                        //TODO: Смотреть флов аргумента вместо проверки на имя
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
        private void ProcessRecordInherit(INamedTypeSymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }
            if (symbol.SpecialType != SpecialType.None)
            {
                return;
            }
            if (symbol.TypeKind == TypeKind.Interface)
            {
                return;
            }
            foreach (var member in symbol.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
                    (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field))
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
                if ((member is IPropertySymbol { SetMethod: { } /*{ IsInitOnly: false }*/, GetMethod: { }, IsReadOnly: false }) ||
                    (member is IFieldSymbol { IsReadOnly: false }))
                {
                    Members.Add(new MemberContext(this, member));
                }

            }
            ProcessRecordInherit(symbol.BaseType);
        }
    }
}
