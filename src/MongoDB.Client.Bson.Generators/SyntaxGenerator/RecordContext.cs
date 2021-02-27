using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class RecordContext : ContextCore
    {
        public RecordContext(MasterContext root, SyntaxNode node, INamedTypeSymbol symbol) : base(root, node, symbol)
        {
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
