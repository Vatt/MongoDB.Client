using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static SyntaxToken TryGetSerializerToken = Identifier("TryGetSerializer");
        public static ExpressionSyntax SerializerMapId = IdentifierName("MongoDB.Client.Bson.Serialization.SerializersMap");
        public static SyntaxToken SerializerInterfaceToken = Identifier("IBsonSerializer");
        public static BaseTypeSyntax SerializerBaseType(ContextCore ctx) 
        {
            //return GenericName(SerializerInterfaceToken, SF.ParseTypeName(ctx.Declaration.ToString()));
            return SF.SimpleBaseType(GenericName(SerializerInterfaceToken, SF.ParseTypeName(ctx.Declaration.ToString())));
        }
        public static BaseTypeSyntax SerializerBaseType(ISymbol sym)
        {
            //return GenericName(SerializerInterfaceToken, SF.ParseTypeName(ctx.Declaration.ToString()));
            return SF.SimpleBaseType(GenericName(SerializerInterfaceToken, SF.ParseTypeName(sym.ToString())));
        }
        public static string SelfName(ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedSym && namedSym.TypeArguments.Length > 0)
            {
                return GenericName(Identifier(namedSym.Name), namedSym.TypeArguments.Select(sym => TypeFullName(sym)).ToArray()).ToString();
            }
            else
            {
                return symbol.Name;
            }
        }
        //public static string SelfFullName(ISymbol symbol) //TODO: починить для генериков
        //{
        //    if (symbol is INamedTypeSymbol namedSym && namedSym.TypeArguments.Length > 0)
        //    {
        //        return GenericName(SF.Identifier(namedSym.ToString()), namedSym.TypeArguments.Select(sym => TypeFullName(sym)).ToArray()).ToString();
        //    }
        //    else
        //    {
        //        return symbol.ToString();
        //    }
        //}
        public static SyntaxToken StaticEnumFieldNameToken(ISymbol enumTypeName, string alias)
        {
            return Identifier($"{enumTypeName.Name}{alias}");
        }
        public static MemberDeclarationSyntax GenerateSerializer(ContextCore ctx)
        {
            MemberDeclarationSyntax declaration = default;
            SyntaxTokenList modifiers;
            if (ctx.Declaration.TypeKind == TypeKind.Struct && ctx.Declaration.IsReadOnly)
            {
                modifiers = new(PublicKeyword(), ReadOnlyKeyword(), PartialKeyword());
            }
            else
            {
                modifiers = new(PublicKeyword(), PartialKeyword());
            }
            switch (ctx.DeclarationNode)
            {
                case ClassDeclarationSyntax:
                    declaration = SF.ClassDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(modifiers)
                        .AddBaseListTypes(SerializerBaseType(ctx))
                        .AddMembers(GenerateStaticNamesSpans(ctx))
                        .AddMembers(GenerateEnumsStaticNamesSpansIfHave(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddMembers(GenerateCollectionMethods(ctx))
                        .AddMembers(GenerateReadStringReprEnumMethods(ctx))
                        .AddMembers(GenerateWriteStringReprEnumMethods(ctx));
                    break;
                case StructDeclarationSyntax:
                    declaration = SF.StructDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(modifiers)
                        .AddBaseListTypes(SerializerBaseType(ctx))
                        .AddMembers(GenerateStaticNamesSpans(ctx))
                        .AddMembers(GenerateEnumsStaticNamesSpansIfHave(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddMembers(GenerateCollectionMethods(ctx))
                        .AddMembers(GenerateReadStringReprEnumMethods(ctx))
                        .AddMembers(GenerateWriteStringReprEnumMethods(ctx));
                    break;
                case RecordDeclarationSyntax decl:
                    var members = new SyntaxList<MemberDeclarationSyntax>()
                        .AddRange(new List<MemberDeclarationSyntax>
                        {
                            TryParseMethod(ctx), WriteMethod(ctx)
                        })
                        .AddRange(GenerateStaticNamesSpans(ctx))
                        .AddRange(GenerateEnumsStaticNamesSpansIfHave(ctx))
                        .AddRange(GenerateCollectionMethods(ctx))
                        .AddRange(GenerateReadStringReprEnumMethods(ctx))
                        .AddRange(GenerateWriteStringReprEnumMethods(ctx));
                    declaration = SF.RecordDeclaration(
                        default,
                        modifiers,
                        decl.Keyword,
                        Identifier(SelfName(ctx.Declaration)),
                        default, default, SF.BaseList(SeparatedList(SerializerBaseType(ctx))), default,
                        OpenBraceToken(), members, CloseBraceToken(), default);

                    break;
            }
            return SF.NamespaceDeclaration(SF.ParseName(ctx.Declaration.ContainingNamespace.ToString())).AddMembers(ProcessNested(declaration, ctx.Declaration.ContainingSymbol));

            static MemberDeclarationSyntax ProcessNested(MemberDeclarationSyntax member, ISymbol symbol)
            {
                MemberDeclarationSyntax decl = default;
                if (symbol is null || symbol.Kind == SymbolKind.Namespace)
                {
                    return member;
                }
                if (symbol is INamedTypeSymbol namedSym && namedSym.GetMembers().Any(x => x.Kind == SymbolKind.Property && x.Name.Equals("EqualityContract", System.StringComparison.InvariantCulture) && x.IsImplicitlyDeclared)) // record shit check
                {
                    TypeParameterSyntax[] typeParams = namedSym.TypeArguments.IsEmpty ? null : namedSym.TypeArguments.Select(x => TypeParameter(x)).ToArray();
                    decl = SF.RecordDeclaration(
                         default,
                         new(PublicKeyword(), PartialKeyword()),
                         RecordKeyword(),
                         Identifier(SelfName(namedSym)),
                         default, default, SF.BaseList(SeparatedList(SerializerBaseType(symbol))), default,
                         OpenBraceToken(), new SyntaxList<MemberDeclarationSyntax>().Add(member), CloseBraceToken(), default);
                    //((RecordDeclarationSyntax)decl).AddTypeParameterListParameters
                    return ProcessNested(decl, symbol.ContainingSymbol);
                }

                switch (((INamedTypeSymbol)symbol).TypeKind)
                {
                    case TypeKind.Class:
                        namedSym = (INamedTypeSymbol)symbol;
                        TypeParameterSyntax[] typeParams = namedSym.TypeArguments.IsEmpty ? null : namedSym.TypeArguments.Select(x => TypeParameter(x)).ToArray();
                        decl = SF.ClassDeclaration(symbol.Name)
                            .AddBaseListTypes(SerializerBaseType(symbol))
                            .AddModifiers(PublicKeyword(), PartialKeyword())
                            .AddMembers(member);
                        if (typeParams is not null)
                        {
                            decl = ((ClassDeclarationSyntax)decl).AddTypeParameterListParameters(typeParams);
                        }

                        break;
                    case TypeKind.Struct:
                        namedSym = (INamedTypeSymbol)symbol;
                        typeParams = namedSym.TypeArguments.IsEmpty ? null : namedSym.TypeArguments.Select(x => TypeParameter(x)).ToArray();
                        decl = SF.StructDeclaration(symbol.Name)
                            .AddBaseListTypes(SerializerBaseType(symbol))
                            .AddModifiers(PublicKeyword(), StaticKeyword())
                            .AddMembers(member);
                        if (typeParams is not null)
                        {
                            decl = ((StructDeclarationSyntax)decl).AddTypeParameterListParameters(typeParams);
                        }
                        break;
                }
                return ProcessNested(decl, symbol.ContainingSymbol);
            }
        }
        static MemberDeclarationSyntax[] GenerateStaticNamesSpans(ContextCore ctx)
        {
            var list = new List<MemberDeclarationSyntax>();
            foreach (var member in ctx.Members)
            {
                var bytes = member.ByteName;
                list.Add(
                    SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PrivateKeyword(), StaticKeyword()),
                        type: ReadOnlySpanByteName,
                        explicitInterfaceSpecifier: default,
                        identifier: member.StaticSpanNameToken,
                        accessorList: default,
                        expressionBody: SF.ArrowExpressionClause(SingleDimensionByteArrayCreation(bytes.Length, SeparatedList(bytes.ToArray().Select(NumericLiteralExpr)))),
                        initializer: default,
                        semicolonToken: SemicolonToken()));
            }
            return list.ToArray();
        }
        static MemberDeclarationSyntax[] GenerateEnumsStaticNamesSpansIfHave(ContextCore ctx)
        {
            var declarations = new List<MemberDeclarationSyntax>();
            var declared = new HashSet<ISymbol>();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (trueType.TypeKind == TypeKind.Enum)
                {
                    int repr = GetEnumRepresentation(member.NameSym);
                    if (repr != 1) // static name spans only for string representation
                    {
                        continue;
                    }
                    if (declared.Contains(trueType))
                    {
                        continue;
                    }
                    var typedMetadata = trueType as INamedTypeSymbol;
                    if (typedMetadata == null)
                    {
                        ReportUnhandledException(nameof(GenerateEnumsStaticNamesSpansIfHave), member.NameSym);
                    }
                    foreach (var enumMember in typedMetadata.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
                    {
                        var (bsonValue, bsonAlias) = GetMemberAlias(enumMember);
                        var bytes = Encoding.UTF8.GetBytes(bsonValue);
                        declarations.Add(
                            SF.PropertyDeclaration(
                                attributeLists: default,
                                modifiers: new(PrivateKeyword(), StaticKeyword()),
                                type: ReadOnlySpanByteName,
                                explicitInterfaceSpecifier: default,
                                identifier: StaticEnumFieldNameToken(typedMetadata, bsonAlias),
                                accessorList: default,
                                expressionBody: SF.ArrowExpressionClause(SingleDimensionByteArrayCreation(bytes.Length, SeparatedList(bytes.Select(NumericLiteralExpr)))),
                                initializer: default,
                                semicolonToken: SemicolonToken()));
                    }
                    declared.Add(trueType);
                }
            }
            return declarations.ToArray();
        }
    }
}
