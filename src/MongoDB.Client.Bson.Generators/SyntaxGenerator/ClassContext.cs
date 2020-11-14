using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ClassContext> Contexts;
        public readonly GeneratorExecutionContext GeneratorExecutionContext;
        public MasterContext(List<INamedTypeSymbol> symbols, GeneratorExecutionContext ctx)
        {
            Contexts = new List<ClassContext>();
            GeneratorExecutionContext = ctx;
            foreach (var symbol in symbols)
            {
                Contexts.Add(new ClassContext(this, symbol));
            }
        }
    }
    internal class ClassContext
    {
        internal IdentifierNameSyntax BsonReaderId => SF.IdentifierName("reader");
        internal IdentifierNameSyntax BsonWriterId => SF.IdentifierName("writer");
        internal IdentifierNameSyntax TryParseOutVar => SF.IdentifierName("message");
        internal IdentifierNameSyntax WriterInputVar => SF.IdentifierName("message");
        
        internal readonly MasterContext Root;
        internal readonly INamedTypeSymbol Declaration;
        internal readonly List<MemberContext> Members;

        internal readonly ImmutableArray<ITypeSymbol>? GenericArgs;
        internal readonly ImmutableArray<IParameterSymbol>? ConstructorParams;
        
        public ClassContext(MasterContext root, INamedTypeSymbol symbol)
        {
            Root = root;
            Declaration = symbol;
            Members = new List<MemberContext>();
            GenericArgs = !Declaration.TypeArguments.IsEmpty ? Declaration.TypeArguments : ImmutableArray<ITypeSymbol>.Empty;
            if (AttributeHelper.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Arity != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }
            foreach (var member in Declaration.GetMembers())
            {
                if (member.IsStatic || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
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
                        //TODO: Сомтреть флов аргумента вместо проверки на имя
                        if (param.Name.Equals(member.Name))
                        {
                            Members.Add(new MemberContext(this, member));
                            break;
                        }
                    }
                    continue;
                } 
                Members.Add(new MemberContext(this, member));
            }
        }
    }
    internal class MemberContext
    {
        internal ClassContext Root { get; }
        internal readonly ISymbol NameSym;
        internal readonly ITypeSymbol TypeSym;
        internal readonly string BsonElementAlias;
        internal readonly string BsonElementValue;
        internal readonly ImmutableArray<ITypeSymbol>? TypeGenericArgs;

        public MemberContext(ClassContext root, ISymbol memberSym)
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
                TypeGenericArgs = !namedType.TypeArguments.IsEmpty ? namedType.TypeArguments : ImmutableArray<ITypeSymbol>.Empty;
            }

            (BsonElementValue, BsonElementAlias) = AttributeHelper.GetMemberAlias(NameSym);
        }
    }
}