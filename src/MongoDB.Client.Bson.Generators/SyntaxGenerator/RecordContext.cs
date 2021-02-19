using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;

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
            if (SerializerGenerator.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }
            else
            {
                if (Declaration.Constructors.Length == 2)
                {
                    ConstructorParams = Declaration.Constructors.Where(x => !SymbolEqualityComparer.Default.Equals(x.Parameters[0].Type, Declaration)).First().Parameters;
                }
            }
            foreach (var member in Declaration.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || SerializerGenerator.IsIgnore(member) ||
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
                //TODO: допустимо только без гетера, если нет сетера проверить есть ли он в конструкторе
                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                     (member is IFieldSymbol { IsReadOnly: false }))
                {
                    Members.Add(new MemberContext(this, member));
                }
            }
            //ProcessRecordInherit(symbol.BaseType);
        }
        //private void ProcessRecordInherit(INamedTypeSymbol symbol)
        //{
        //    if (symbol == null)
        //    {
        //        return;
        //    }
        //    if (symbol.SpecialType != SpecialType.None)
        //    {
        //        return;
        //    }
        //    if (symbol.TypeKind == TypeKind.Interface)
        //    {
        //        return;
        //    }
        //    foreach (var member in symbol.GetMembers())
        //    {
        //        if (Members.Any(m=>m.NameSym.Name.Equals(member.Name)))
        //        {
        //            continue;
        //        }
        //        if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
        //            (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field))
        //        {
        //            continue;
        //        }
        //        if (member.DeclaredAccessibility != Accessibility.Public)
        //        {
        //            if (ConstructorParams == null)
        //            {
        //                continue;
        //            }

        //            foreach (var param in ConstructorParams)
        //            {
        //                if (param.Name.Equals(member.Name))
        //                {
        //                    Members.Add(new MemberContext(this, member));
        //                    break;
        //                }
        //            }
        //            continue;
        //        }
        //        if ((member is IPropertySymbol { SetMethod: { } /*{ IsInitOnly: false }*/, GetMethod: { }, IsReadOnly: false }) ||
        //            (member is IFieldSymbol { IsReadOnly: false }))
        //        {
        //            Members.Add(new MemberContext(this, member));
        //        }

        //    }
        //    ProcessRecordInherit(symbol.BaseType);
        //}
    }
}
