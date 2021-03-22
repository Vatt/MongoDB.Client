using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class GeneratorMode
    {
        [Flags]
        private enum PrivateMode : byte
        {
            IfConditions = 1,
            ConstuctorOnlyParameters = 2,
        }
        public bool IfConditions { get; set; }
        public bool ConstructorOnlyParameters { get; }
        public GeneratorMode(byte byteMode)
        {
            PrivateMode mode = (PrivateMode)byteMode;
            
            //IfConditions = (mode | PrivateMode.IfConditions) == PrivateMode.IfConditions;
            //ConstructorOnlyParameters = (mode | PrivateMode.ConstuctorOnlyParameters) == PrivateMode.ConstuctorOnlyParameters;   
            IfConditions = mode.HasFlag(PrivateMode.IfConditions);
            ConstructorOnlyParameters = mode.HasFlag(PrivateMode.ConstuctorOnlyParameters);
        }
        public GeneratorMode()
        {
            IfConditions = false;
            ConstructorOnlyParameters = false;
        }
    }
    internal class ContextCore
    {
        internal MasterContext Root { get; }
        internal INamedTypeSymbol Declaration { get; }
        internal SyntaxNode DeclarationNode { get; }
        internal List<MemberContext> Members { get; }
        internal ImmutableArray<ITypeSymbol>? GenericArgs { get; }
        internal ImmutableArray<IParameterSymbol>? ConstructorParams { get; }
        internal GeneratorMode GeneratorMode { get; }
        internal SyntaxToken SerializerName
        {
            get
            {
                string name = GenericArgs.HasValue ? Declaration.Name + string.Join(string.Empty, GenericArgs.Value) : Declaration.Name;
                ISymbol sym = Declaration;
                while (sym.ContainingSymbol.Kind != SymbolKind.Namespace)
                {
                    string generics = string.Empty;
                    if (sym.ContainingSymbol is INamedTypeSymbol namedType)
                    {
                        if (namedType.TypeArguments.IsEmpty == false)
                        {
                            generics = string.Join(string.Empty, namedType.TypeArguments);
                        }
                    }
                    name += "." + sym.ContainingSymbol.Name + generics;
                    sym = sym.ContainingSymbol;
                }
                return SerializerGenerator.Identifier($"{name}.g");
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
            if (SerializerGenerator.TryFindPrimaryConstructor(Declaration, node, out var constructor))
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

                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                     (member is IFieldSymbol { IsReadOnly: false, IsConst: false }))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
                if (member is IPropertySymbol prop && (prop.IsReadOnly || prop.GetMethod != null) && ConstructorContains(member.Name))
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
            GeneratorMode = SerializerGenerator.GetGeneratorMode(symbol);
            if (Members.Count <= 2)
            {
                GeneratorMode.IfConditions = true;
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
