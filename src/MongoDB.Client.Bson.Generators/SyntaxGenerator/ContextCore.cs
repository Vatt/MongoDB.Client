using Microsoft.CodeAnalysis;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class NameStatistics
    {
        internal Dictionary<int, int> LengthMap { get; }
        internal float UniqueNamesLengthRatio { get; }
        internal int AvgLenght { get; }
        internal int MinLength { get; }
        internal int MaxLength { get; }
        internal NameStatistics(Dictionary<int, int> lenMap)
        {
            LengthMap = lenMap;
            UniqueNamesLengthRatio = LengthMap.Values.Where(l => l == 1).Count() / (float)lenMap.Values.Sum();
            AvgLenght = LengthMap.Select(kv => kv.Key * kv.Value).Sum() / LengthMap.Values.Sum();
            MinLength = LengthMap.Keys.Min();
            MaxLength = LengthMap.Keys.Max();
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
        internal NameStatistics NameStatistics { get; }
        internal int GeneratorMode { get; }
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
            GeneratorMode = Members.Count == 1 ? 1 : SerializerGenerator.GetGeneratorMode(symbol);
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
