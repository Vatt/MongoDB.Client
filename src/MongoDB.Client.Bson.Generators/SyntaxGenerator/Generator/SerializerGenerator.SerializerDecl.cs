using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static StatementSyntax[] OtherTryParseBson(MemberContext member)
        {
            var genericName = GenericName(SF.Identifier("TryGetSerializer"), TypeFullName(member.TypeSym));
            var serializersMapCall = InvocationExpr(IdentifierName("MongoDB.Client.Bson.Serialization.SerializersMap"),
                                                    genericName,
                                                    OutArgument(VarVariableDeclarationExpr(SF.Identifier($"{member.NameSym.Name}Serializer"))));
            var serializerTryParse = InvocationExpr(SF.IdentifierName($"{member.NameSym.Name}Serializer"), IdentifierName("TryParseBson"), RefArgument(member.Root.BsonReaderToken), OutArgument(IdentifierName(member.AssignedVariable)));
            return Statements(
                IfNot(serializersMapCall, SerializerNotFoundException(member.TypeSym)),
                IfNotReturnFalse(serializerTryParse));
        }
        public static StatementSyntax[] OtherWriteBson(MemberContext member)
        {
            var genericName = GenericName(SF.Identifier("TryGetSerializer"), TypeFullName(member.TypeSym));
            var serializersMapCall = InvocationExpr(IdentifierName("MongoDB.Client.Bson.Serialization.SerializersMap"),
                                                    genericName,
                                                    OutArgument(VarVariableDeclarationExpr(SF.Identifier($"{member.NameSym.Name}Serializer"))));
            var sma = SimpleMemberAccess(member.Root.WriterInputVar, IdentifierName(member.NameSym));
            var serializerWrite = InvocationExprStatement(SF.IdentifierName($"{member.NameSym.Name}Serializer"), IdentifierName("WriteBson"), RefArgument(member.Root.BsonWriterToken), Argument(sma));
            return Statements(
                IfNot(serializersMapCall, SerializerNotFoundException(member.TypeSym)),
                serializerWrite);
        }
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

        public static string SerializerName(ContextCore ctx)
        {
            string generics = ctx.GenericArgs.HasValue && ctx.GenericArgs.Value.Length > 0
                ? string.Join(String.Empty, ctx.GenericArgs.Value)
                : String.Empty;
            return $"{ctx.Declaration.ContainingNamespace.ToString().Replace(".", String.Empty)}{ctx.Declaration.Name}{generics}SerializerGenerated";
        }
        public static SyntaxToken StaticFieldNameToken(MemberContext ctx)
        {
            return SF.Identifier($"{ctx.Root.Declaration.Name}{ctx.BsonElementAlias}");
        }
        public static SyntaxToken StaticEnumFieldNameToken(ISymbol enumTypeName, string alias)
        {
            return SF.Identifier($"{enumTypeName.Name}{alias}");
        }
        public static MemberDeclarationSyntax GenerateSerializer(ContextCore ctx)
        {
            MemberDeclarationSyntax declaration = default;
            switch (ctx.DeclarationNode)
            {
                case ClassDeclarationSyntax:
                    declaration = SF.ClassDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(new(PublicKeyword(), PartialKeyword()))
                        .AddMembers(GenerateStaticNamesSpans(ctx))
                        .AddMembers(GenerateEnumsStaticNamesSpansIfHave(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddMembers(GenerateReadArrayMethods(ctx))
                        .AddMembers(GenerateWriteArrayMethods(ctx))
                        .AddMembers(GenerateReadStringReprEnumMethods(ctx))
                        .AddMembers(GenerateWriteStringReprEnumMethods(ctx));
                    break;
                case StructDeclarationSyntax:
                    declaration = SF.StructDeclaration(SelfName(ctx.Declaration))
                        .WithModifiers(new(PublicKeyword(), PartialKeyword()))
                        .AddMembers(GenerateStaticNamesSpans(ctx))
                        .AddMembers(GenerateEnumsStaticNamesSpansIfHave(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddMembers(GenerateReadArrayMethods(ctx))
                        .AddMembers(GenerateWriteArrayMethods(ctx))
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
                        .AddRange(GenerateReadArrayMethods(ctx))
                        .AddRange(GenerateWriteArrayMethods(ctx))
                        .AddRange(GenerateReadStringReprEnumMethods(ctx))
                        .AddRange(GenerateWriteStringReprEnumMethods(ctx));
                    declaration = SF.RecordDeclaration(
                        default,
                        new(PublicKeyword(), PartialKeyword()),
                        decl.Keyword,
                        SF.Identifier(SelfName(ctx.Declaration)),
                        default, default, default, default,
                        OpenBraceToken(), members, CloseBraceToken(), default);

                    break;
            }
            return SF.NamespaceDeclaration(SF.ParseName(ctx.Declaration.ContainingNamespace.ToString())).AddMembers(ProcessNested(declaration, ctx.Declaration.ContainingSymbol)); ;

            static MemberDeclarationSyntax ProcessNested(MemberDeclarationSyntax member, ISymbol symbol)
            {
                MemberDeclarationSyntax decl = default;
                if (symbol is null || symbol.Kind == SymbolKind.Namespace)
                {
                    return member;
                }
                if (symbol is INamedTypeSymbol namedSym && namedSym.GetMembers().Any(x => x.Kind == SymbolKind.Property && x.Name == "EqualityContract" && x.IsImplicitlyDeclared)) // record shit check
                {
                    decl = SF.RecordDeclaration(
                         default,
                         new(PublicKeyword(), PartialKeyword()),
                         RecordKeyword(),
                         SF.Identifier(SelfName(namedSym)),
                         default, default, default, default,
                         OpenBraceToken(), new SyntaxList<MemberDeclarationSyntax>().Add(member), CloseBraceToken(), default);
                    return ProcessNested(decl, symbol.ContainingSymbol);
                }

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
                }
                return ProcessNested(decl, symbol.ContainingSymbol);
            }
        }
        static MemberDeclarationSyntax[] GenerateStaticNamesSpans(ContextCore ctx)
        {
            var list = new List<MemberDeclarationSyntax>();
            foreach (var member in ctx.Members)
            {
                var bytes = Encoding.UTF8.GetBytes(member.BsonElementValue);
                list.Add(
                    SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PrivateKeyword(), StaticKeyword()),
                        type: ReadOnlySpanByte(),
                        explicitInterfaceSpecifier: default,
                        identifier: StaticFieldNameToken(member),
                        accessorList: default,
                        expressionBody: SF.ArrowExpressionClause(SingleDimensionByteArrayCreation(bytes.Length, SeparatedList(bytes.Select(NumericLiteralExpr)))),
                        initializer: default,
                        semicolonToken: SemicolonToken()));
            }
            return list.ToArray();
        }
        static MemberDeclarationSyntax[] GenerateEnumsStaticNamesSpansIfHave(ContextCore ctx)
        {
            Dictionary<ISymbol, List<MemberDeclarationSyntax>> declarations = new();
            foreach (var member in ctx.Members)
            {

                if (member.TypeSym.TypeKind == TypeKind.Enum)
                {
                    int repr = AttributeHelper.GetEnumRepresentation(member.NameSym);
                    if (repr != 1) // static name spans only for string representation
                    {
                        continue;
                    }
                    if (declarations.ContainsKey(member.TypeSym))
                    {
                        continue;
                    }
                    var typedMetadata = member.TypeMetadata as INamedTypeSymbol;
                    if (typedMetadata == null)
                    {
                        GeneratorDiagnostics.ReportUnhandledException(nameof(GenerateEnumsStaticNamesSpansIfHave), member.NameSym);
                    }
                    declarations[member.TypeSym] = new();
                    foreach (var enumMember in typedMetadata.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
                    {
                        var list = new List<MemberDeclarationSyntax>();
                        var (bsonValue, bsonAlias) = AttributeHelper.GetMemberAlias(enumMember);
                        var bytes = Encoding.UTF8.GetBytes(bsonValue);
                        declarations[member.TypeSym].Add(
                            SF.PropertyDeclaration(
                                attributeLists: default,
                                modifiers: new(PrivateKeyword(), StaticKeyword()),
                                type: ReadOnlySpanByte(),
                                explicitInterfaceSpecifier: default,
                                identifier: StaticEnumFieldNameToken(typedMetadata, bsonAlias),
                                accessorList: default,
                                expressionBody: SF.ArrowExpressionClause(SingleDimensionByteArrayCreation(bytes.Length, SeparatedList(bytes.Select(NumericLiteralExpr)))),
                                initializer: default,
                                semicolonToken: SemicolonToken()));
                    }
                }
            }
            var result = new List<MemberDeclarationSyntax>();
            foreach (var value in declarations.Values)
            {
                result.AddRange(value);
            }
            return result.ToArray();
        }
    }
}