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
        internal INamedTypeSymbol BaseSym { get; }
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
            BaseSym = Declaration.BaseType;
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
            GeneratorMode = SerializerGenerator.GetGeneratorMode(symbol);

            if (GeneratorMode.ConstructorOnlyParameters)
            {
                CreateContructorOnlyMembers();
            }
            else
            {
                CreateDefaultMembers();
            }
            
            if (Members.Count <= 2)
            {
                GeneratorMode.IfConditions = true;
            }
        }
        public void CreateDefaultMembers()
        {
            foreach (var member in Declaration.GetMembers())
            {
                if (IsBadMemberSym(member))
                {
                    continue;
                }

                if (CheckAccessibility(member))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
                if (CheckGetAccessibility(member) && ConstructorContains(member.Name))
                {
                    Members.Add(new MemberContext(this, member));
                    continue;
                }
            }
        }
        public void CreateContructorOnlyMembers()
        {
            if (ConstructorParams.HasValue == false)
            {
                //TODO: report diagnostic error
                return;
            }
            foreach(var param in ConstructorParams)
            {
                if (MembersContains(param, out var namedType))
                {
                    Members.Add(new MemberContext(this, namedType));
                }
                
            }
        }

        public bool MembersContains(IParameterSymbol parameter, out ISymbol symbol)
        {
            symbol = Declaration.GetMembers()
                .Where(sym => IsBadMemberSym(sym) == false)
                .FirstOrDefault(sym => CheckGetAccessibility(sym) &&
                                       ExtractTypeFromSymbol(sym, out var type) && 
                                       sym.Name.Equals(parameter.Name) && 
                                       type.Equals(parameter.Type, SymbolEqualityComparer.Default));
            if (symbol is not null)
            {
                return true;
            }

            if (BaseSym is not null)
            {
                return InheritMemberContains(parameter, BaseSym, out symbol);
            }

            return false;
        }
        private bool InheritMemberContains(IParameterSymbol parameter, INamedTypeSymbol symbol, out ISymbol result)
        {
            var baseSym = symbol.BaseType;
            result = symbol.GetMembers()
                .Where(sym => IsBadMemberSym(sym) == false)
                .FirstOrDefault(sym => CheckGetAccessibility(sym) &&
                                       ExtractTypeFromSymbol(sym, out var type) && 
                                       sym.Name.Equals(parameter.Name) && 
                                       type.Equals(parameter.Type, SymbolEqualityComparer.Default));
            if (result is not null)
            {
                return true;
            }

            if (baseSym is not null)
            {
                return InheritMemberContains(parameter, baseSym, out result);
            }

            return false;
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

        private static bool ExtractTypeFromSymbol(ISymbol sym, out ITypeSymbol type)
        {
            switch (sym)
            {
                case IFieldSymbol field:
                    type = field.Type;
                    return true;
                case IPropertySymbol prop:
                    type = prop.Type;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }

        private static bool CheckAccessibility(ISymbol member)
        {
            if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                (member is IFieldSymbol { IsReadOnly: false, IsConst: false }))
            {
                return true;
            }

            return false;
        }

        private static bool CheckGetAccessibility(ISymbol member)
        {
            if (member is IPropertySymbol prop && (prop.IsReadOnly || prop.GetMethod != null))
            {
                return true;
            }
            if (member is IFieldSymbol {IsReadOnly: true, IsConst: false})
            {
                return true;
            }

            return false;
        }
        private static bool IsBadMemberSym(ISymbol member)
        {
            if ( member.IsAbstract ||
                 SerializerGenerator.IsIgnore(member) || 
                 (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field) ||
                 member.DeclaredAccessibility != Accessibility.Public)
            {
                return true;
            }

            return false;
        }
    }
}
