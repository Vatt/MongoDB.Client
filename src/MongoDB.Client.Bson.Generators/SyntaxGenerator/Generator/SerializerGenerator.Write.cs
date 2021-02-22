﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static MethodDeclarationSyntax WriteMethod(ContextCore ctx)
        {
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: new(PublicKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,// SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(ctx.Declaration))),
                    returnType: VoidPredefinedType(),
                    identifier: Identifier("WriteBson"),
                    parameterList: ParameterList(
                        RefParameter(ctx.BsonWriterType, ctx.BsonWriterToken),
                        InParameter(TypeFullName(ctx.Declaration), ctx.TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(WriteMethodBody(ctx));
        }
        public static StatementSyntax GenerateWriteEnum(ContextCore ctx, MemberContext member, ExpressionSyntax writeTarget)
        {
            int repr = GetEnumRepresentation(member.NameSym);
            if (repr == -1) { repr = 2; }
            if (repr != 1)
            {
                return Statement(Write_Type_Name_Value(StaticFieldNameToken(member), repr == 2 ? CastToInt(writeTarget) : CastToLong(writeTarget)));
            }
            else
            {
                var methodName = IdentifierName(WriteStringReprEnumMethodName(ctx, member.TypeMetadata, member.NameSym));
                return InvocationExprStatement(methodName, RefArgument(ctx.BsonWriterToken), Argument(StaticFieldNameToken(member)), Argument(writeTarget));
            }
        }
        private static BlockSyntax WriteMethodBody(ContextCore ctx)
        {
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            StatementsBuilder statements = new();

            foreach (var member in ctx.Members)
            {
                StatementsBuilder builder = new();
                var writeTarget = SimpleMemberAccess(ctx.WriterInputVar, IdentifierName(member.NameSym));
                ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                TryGetBsonWriteIgnoreIfAttr(member, out var condition);
                if (member.TypeSym.TypeKind == TypeKind.Enum)
                {
                    builder.Statements(GenerateWriteEnum(ctx, member, writeTarget));
                    goto CONDITION_CHECK;
                }
                if (member.TypeSym.IsReferenceType)
                {
                    builder.IfStatement(
                                condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                statement: Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                                @else: Block(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, trueType, ctx.BsonWriterId, writeTarget)));
                }
                else if (IsBsonSerializable(trueType) && member.TypeSym.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    builder.IfStatement(
                                condition: BinaryExprEqualsEquals(SimpleMemberAccess(writeTarget, IdentifierName("HasValue")), FalseLiteralExpr()),
                                statement: Block(WriteBsonNull(StaticFieldNameToken(member))),
                                @else: Block(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, member.TypeSym, ctx.BsonWriterId, writeTarget)));
                }
                else if (IsBsonSerializable(trueType) && member.TypeSym.NullableAnnotation != NullableAnnotation.Annotated)
                {
                    builder.Statements(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, member.TypeSym, ctx.BsonWriterId, writeTarget));
                }
                else
                {
                    if (member.BsonElementAlias.Equals("_id") && TypeLib.IsBsonObjectId(trueType))
                    {
                        statements.IfStatement(
                                    BinaryExprEqualsEquals(writeTarget, Default(TypeFullName(trueType))),
                                    Block(SimpleAssignExpr(writeTarget, NewBsonObjectId())));
                    }
                    builder.Statements(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, trueType, ctx.BsonWriterId, writeTarget));
                }
            CONDITION_CHECK:
                if (condition != null)
                {
                    statements.IfNot(condition, Block(builder.Build()));
                }
                else
                {
                    statements.Statements(builder);
                }
            }

            return Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)))
                .AddStatements(
                    statements.Build())
                .AddStatements(
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), checkpoint)),
                    LocalDeclarationStatement(SpanByte(), sizeSpan, StackAllocByteArray(4)),
                    Statement(BinaryPrimitivesWriteInt32LittleEndian(sizeSpan, docLength)),
                    Statement(ReservedWrite(reserved, sizeSpan)),
                    Statement(WriterCommit())
                    );
        }

        public static StatementSyntax[] WriteOperation(MemberContext ctx, SyntaxToken name, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax writerId, ExpressionSyntax writeTarget)
        {
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (TryGetSimpleWriteOperation(nameSym, trueType, name, writeTarget, out var expr))
            {

                return new[] { Statement(expr) };
            }
            if (ctx.Root.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueType.Name)) != default)
            {
                return Statements
                (
                    VarLocalDeclarationStatement(SF.Identifier($"{name}genericReserved"), WriterReserve(1)),
                    Statement(WriteCString(StaticFieldNameToken(ctx))),
                    Statement(WriteGeneric(writeTarget, SF.IdentifierName($"{name}genericReserved")))
                );
            }
            if (trueType is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (TypeLib.IsListOrIList(namedType))
                {
                    return Statements
                    (
                            Statement(Write_Type_Name(4, name)),
                            InvocationExprStatement(WriteArrayMethodName(ctx, trueType), RefArgument(writerId), Argument(writeTarget))
                    );
                }
            }
            if (IsBsonSerializable(trueType))
            {
                var target = typeSym.NullableAnnotation == NullableAnnotation.Annotated && typeSym.TypeKind == TypeKind.Struct ? SimpleMemberAccess(writeTarget, IdentifierName("Value")) : writeTarget;
                return Statements
                (
                       Statement(Write_Type_Name(3, StaticFieldNameToken(ctx))),
                       InvocationExprStatement(IdentifierName(trueType.ToString()), IdentifierName("WriteBson"), RefArgument(writerId), Argument(target))
                );
            }
            else
            {
                GeneratorDiagnostics.ReportSerializerMapUsingWarning(ctx.NameSym);
                return Statements(
                        Statement(Write_Type_Name(3, StaticFieldNameToken(ctx))),
                        OtherWriteBson(ctx)
                    );
            }
        }
        private static bool TryGetSimpleWriteOperation(ISymbol nameSym, ITypeSymbol typeSymbol, SyntaxToken bsonNameToken, ExpressionSyntax writeTarget, out InvocationExpressionSyntax expr)
        {
            expr = default;
            var bsonName = IdentifierName(bsonNameToken);
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Double:
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case SpecialType.System_String:
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case SpecialType.System_Boolean:
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case SpecialType.System_Int32:
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case SpecialType.System_Int64:
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                    //case SpecialType.System_DateTime:
                    //    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    //    return true;
            }
            if (TypeLib.IsBsonDocument(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (TypeLib.IsBsonArray(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (TypeLib.IsBsonObjectId(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (TypeLib.IsGuid(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (TypeLib.IsDateTimeOffset(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (typeSymbol.SpecialType != SpecialType.None)
            {
                GeneratorDiagnostics.ReportUnsuporterTypeError(nameSym, typeSymbol);
            }
            return false;
        }
    }
}
