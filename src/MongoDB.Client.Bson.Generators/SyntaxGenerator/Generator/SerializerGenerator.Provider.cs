using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static string SelfName(ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedSym && namedSym.TypeArguments.Length > 0)
            {
                return GenericName(SF.Identifier(namedSym.Name), namedSym.TypeArguments.Select(sym => TypeFullName(sym)).ToArray()).ToString();
            }
            else
            {
                return symbol.Name;
            }
        }
        public static string SelfFullName(ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedSym && namedSym.TypeArguments.Length > 0)
            {
                return GenericName(SF.Identifier(namedSym.ToString()), namedSym.TypeArguments.Select(sym => TypeFullName(sym)).ToArray()).ToString();
            }
            else
            {
                return symbol.ToString();
            }
        }
        public static TypeSyntax SelTypefName(ContextCore ctx)
        {
            if (ctx.GenericArgs.HasValue)
            {
                return GenericName(SF.Identifier($"MongoDB.Client.Bson.Serialization.Generated.{SerializerName(ctx)}"), ctx.GenericArgs.Value.Select(sym => TypeFullName(sym)).ToArray());
            }
            else
            {
                return SF.ParseTypeName($"MongoDB.Client.Bson.Serialization.Generated.{SerializerName(ctx)}");
            }
        }
        public static MemberDeclarationSyntax GenerateStaticProvider(ContextCore ctx)
        {
            MemberDeclarationSyntax provider = default;
            var providerDecl = SF.PropertyDeclaration(
                            attributeLists: default,
                            modifiers: new(PublicKeyword(), StaticKeyword()),
                            type: SelTypefName(ctx),
                            explicitInterfaceSpecifier: default,
                            identifier: SF.Identifier("Serializer"),
                            accessorList: default,
                            expressionBody: SF.ArrowExpressionClause(ObjectCreation(SelTypefName(ctx))),
                            initializer: default,
                            semicolonToken: SemicolonToken());
            switch (ctx.DeclarationNode)
            {
                case ClassDeclarationSyntax:
                    provider = SF.ClassDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(new(PublicKeyword(), PartialKeyword()))
                        .AddMembers(providerDecl);
                    break;
                case StructDeclarationSyntax:
                    provider = SF.StructDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(new(PublicKeyword(), PartialKeyword()))
                        .AddMembers(providerDecl); 
                    break;
                case RecordDeclarationSyntax decl:
                    provider = SF.RecordDeclaration(
                        default,
                        new(PublicKeyword(), PartialKeyword()),
                        decl.Keyword, 
                        SF.Identifier(SelfName(ctx.Declaration)),
                        default, default, default, default, 
                        OpenBraceToken(), new(providerDecl), CloseBraceToken(), default);

                    break;
            }
            return SF.NamespaceDeclaration(SF.ParseName(ctx.Declaration.ContainingNamespace.ToString())).AddMembers(ProcessNested(provider, ctx.Declaration.ContainingSymbol));

            static MemberDeclarationSyntax ProcessNested(MemberDeclarationSyntax member, ISymbol symbol)
            {
                if (symbol is null || symbol.Kind == SymbolKind.Namespace)
                {
                    return member;
                }
                MemberDeclarationSyntax decl = default;
                switch (((INamedTypeSymbol)symbol).TypeKind)
                {
                    case TypeKind.Class:
                        decl = SF.ClassDeclaration(symbol.Name)
                            .AddModifiers(PublicKeyword(), PartialKeyword())
                            .AddMembers(member);
                        break;
                    case TypeKind.Struct:
                        decl = SF.StructDeclaration(symbol.Name)
                            .AddModifiers(PublicKeyword(), StaticKeyword())
                            .AddMembers(member);
                        break;
                        //TODO: Record
                }
                return ProcessNested(decl, symbol.ContainingSymbol);
            }
        }
    }
}
