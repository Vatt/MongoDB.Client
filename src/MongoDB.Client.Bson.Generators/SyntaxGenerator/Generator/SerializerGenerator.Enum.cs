﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken ReadStringReprEnumMethodName(ISymbol enumTypeName, ISymbol fieldOrPropertyName)
        {
            var (_, alias) = GetMemberAlias(fieldOrPropertyName);
            return SF.Identifier($"TryParse{enumTypeName.Name}");
        }
        private static SyntaxToken WriteStringReprEnumMethodName(ISymbol enumTypeName, ISymbol fieldOrPropertyName)
        {
            var (_, alias) = GetMemberAlias(fieldOrPropertyName);
            return SF.Identifier($"Write{enumTypeName.Name}");
        }
        private static MethodDeclarationSyntax[] GenerateWriteStringReprEnumMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            HashSet<ISymbol> alreadyCreated = new();
            //foreach (var member in ctx.Members.Where(member => member.TypeSym.TypeKind == TypeKind.Enum))
            foreach (var member in ctx.Members.Where(member => ExtractTypeFromNullableIfNeed(member.TypeSym).TypeKind == TypeKind.Enum))
            {
                var repr = GetEnumRepresentation(member.NameSym);
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (repr == 1 && alreadyCreated.Contains(trueType) == false)
                {
                    methods.Add(WriteStringReprEnumMethod(member));
                    alreadyCreated.Add(trueType);

                }
            }
            return methods.ToArray();
        }
        private static MethodDeclarationSyntax[] GenerateReadStringReprEnumMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            var alreadyCreated = new HashSet<ISymbol>();
            foreach (var member in ctx.Members.Where(member => ExtractTypeFromNullableIfNeed(member.TypeSym).TypeKind == TypeKind.Enum))
            {
                var repr = GetEnumRepresentation(member.NameSym);
                if (repr == 1 && alreadyCreated.Contains(member.TypeSym) == false)
                {
                    methods.Add(ReadStringReprEnumMethod(member));
                    alreadyCreated.Add(member.TypeSym);
                }
            }
            return methods.ToArray();
        }
        private static MethodDeclarationSyntax ReadStringReprEnumMethod(MemberContext ctx)
        {
            var outMessage = Identifier("enumMessage");
            var trueType = ExtractTypeFromNullableIfNeed(ctx.TypeSym);
            var repr = GetEnumRepresentation(ctx.NameSym);
            var alreadyCreated = new HashSet<ISymbol>();
            if (repr != 1)
            {
                return default;
            }
            var stringData = Identifier("stringData");
            List<StatementSyntax> statements = new()
            {
                SimpleAssignExprStatement(outMessage, DefaultLiteralExpr()),
                IfNotReturnFalse(TryGetStringAsSpan(VarVariableDeclarationExpr(stringData)))
            };
            foreach (var member in trueType.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
            {
                var (_, alias) = GetMemberAlias(member);
                statements.Add(
                    SF.IfStatement(
                        condition: SpanSequenceEqual(stringData, StaticEnumFieldNameToken(trueType, alias)),
                        statement:
                        SF.Block(
                            SimpleAssignExprStatement(outMessage, IdentifierFullName(member)),
                            SF.ReturnStatement(TrueLiteralExpr())
                            )));
            }
            statements.Add(SF.ReturnStatement(TrueLiteralExpr()));
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: ReadStringReprEnumMethodName(trueType, ctx.NameSym),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(IdentifierName(ctx.TypeSym.ToString()), outMessage)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(SF.Block(statements.ToArray()));
        }
        private static MethodDeclarationSyntax WriteStringReprEnumMethod(MemberContext ctx)
        {
            var spanNameArg = Identifier("name");
            var trueType = ExtractTypeFromNullableIfNeed(ctx.TypeSym);
            var repr = GetEnumRepresentation(ctx.NameSym);
            if (repr != 1)
            {
                return default;
            }
            List<StatementSyntax> statements = new();
            foreach (var member in trueType.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
            {
                var (_, alias) = GetMemberAlias(member);
                statements.Add(
                    IfStatement(
                        condition: BinaryExprEqualsEquals(WriterInputVarToken, IdentifierFullName(member)),
                        statement: Block(
                            Write_Type_Name(2, spanNameArg),
                            WriteString(StaticEnumFieldNameToken(trueType, alias)))
                    ));
            }
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: WriteStringReprEnumMethodName(trueType, ctx.NameSym),
                    parameterList: ParameterList(RefParameter(BsonWriterType, BsonWriterToken),
                                                 Parameter(ReadOnlySpanByteName, spanNameArg),
                                                 Parameter(TypeFullName(trueType), WriterInputVarToken)),

                    body: Block(statements.ToArray()),
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default);
        }
    }
}