using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken ReadStringReprEnumMethodName(ClassContext ctx, ISymbol enumTypeName, ISymbol fieldOrPropertyName)
        {
            var (_, alias) = AttributeHelper.GetMemberAlias(fieldOrPropertyName);
            return SF.Identifier($"TryParse{enumTypeName.Name}");
        }
        private static SyntaxToken WriteStringReprEnumMethodName(ClassContext ctx, ISymbol enumTypeName, ISymbol fieldOrPropertyName)
        {
            var (_, alias) = AttributeHelper.GetMemberAlias(fieldOrPropertyName);
            return SF.Identifier($"Write{enumTypeName.Name}");
        }
        private static MethodDeclarationSyntax[] GenerateWriteStringReprEnumMethods(ClassContext ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members.Where(member => member.TypeSym.TypeKind == TypeKind.Enum))
            {
                var repr = AttributeHelper.GetEnumRepresentation(member.NameSym);
                if (repr == 1)
                {
                    methods.Add(WriteStringReprEnumMethod(member));
                }
            }
            return methods.ToArray();
        }
        private static MethodDeclarationSyntax[] GenerateReadStringReprEnumMethods(ClassContext ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members.Where(member => member.TypeSym.TypeKind == TypeKind.Enum))
            {
                var repr = AttributeHelper.GetEnumRepresentation(member.NameSym);
                if (repr == 1)
                {
                    methods.Add(ReadStringReprEnumMethod(member));
                }
            }
            return methods.ToArray();
        }
                private static MethodDeclarationSyntax ReadStringReprEnumMethod(MemberContext ctx)
        {
            var outMessage = SF.Identifier("enumMessage");           
            var repr = AttributeHelper.GetEnumRepresentation(ctx.NameSym);
            var metadata = ctx.TypeMetadata as INamedTypeSymbol;
            if (repr != 1 )
            {
                return default;
            }
            var stringData = SF.Identifier("stringData");
            List<StatementSyntax> statements = new()
            {
                SimpleAssignExprStatement(outMessage, DefaultLiteralExpr()),
                IfNotReturnFalse(TryGetStringAsSpan(VarVariableDeclarationExpr(stringData)))
            };
            foreach (var member in metadata.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
            {
                var (_, alias) = AttributeHelper.GetMemberAlias(member);
                statements.Add(
                    SF.IfStatement(
                        condition: SpanSequenceEqual(stringData, StaticEnumFieldNameToken(metadata, alias)),
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
                    identifier: ReadStringReprEnumMethodName(ctx.Root, metadata, ctx.NameSym),
                    parameterList: ParameterList(RefParameter(ctx.Root.BsonReaderType, ctx.Root.BsonReaderToken),
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
            var spanNameArg = SF.Identifier("name");
            var metadata = ctx.TypeMetadata as INamedTypeSymbol;
            var repr = AttributeHelper.GetEnumRepresentation(ctx.NameSym);
            if (repr != 1)
            {
                return default;
            }
            List<StatementSyntax> statements = new();
            foreach (var member in metadata.GetMembers().Where(sym => sym.Kind == SymbolKind.Field))
            {
                var (_, alias) = AttributeHelper.GetMemberAlias(member);               
                statements.Add(
                    SF.IfStatement(
                        condition: BinaryExprEqualsEquals(ctx.Root.WriterInputVar, IdentifierFullName(member)),
                        statement: SF.Block(
                            //Statement(Write_Type_Name(2, IdentifierName(StaticFieldNameToken(ctx)))),
                            Statement(Write_Type_Name(2, spanNameArg)),
                            Statement(WriteString(StaticEnumFieldNameToken(metadata, alias))))
                    ));
            }
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: WriteStringReprEnumMethodName(ctx.Root, metadata, ctx.NameSym),
                    parameterList: ParameterList(RefParameter(ctx.Root.BsonWriterType, ctx.Root.BsonWriterToken),
                                                 Parameter(ReadOnlySpanByte(), spanNameArg),
                                                 Parameter(TypeFullName(ctx.TypeSym), ctx.Root.WriterInputVarToken)),
                                                  
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(SF.Block(statements.ToArray()));
        }
    }
}