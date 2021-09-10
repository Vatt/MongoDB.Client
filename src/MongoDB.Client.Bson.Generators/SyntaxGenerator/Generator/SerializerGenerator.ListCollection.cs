﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly CollectionReadContext ListReadContext = new CollectionReadContext(
            Identifier("listDocLength"),
            Identifier("listUnreaded"),
            Identifier("listEndMarker"),
            Identifier("listBsonType"),
            Identifier("listBsonName"),
            Identifier("list"),
            Identifier("temp"),
            Identifier("internalList"),
            new[] { Argument(Identifier("temp")) });
        private static SwitchStatementSyntax GenerateListStatesSwitch(MemberContext ctx)
        {
            List<SwitchSectionSyntax> sections = new();
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            var trueType = ExtractTypeFromNullableIfNeed(ctx.TypeSym);
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[0];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);
            if (TryGenerateCollectionTryParseBson(ctx, trueTypeArg, ctx.NameSym, ListReadContext, builder, true, true))
            {
                goto SECTIONS;
            }
            else if (TryGenerateCollectionSimpleRead(ctx, trueTypeArg, ListReadContext, builder))
            {
                goto SECTIONS;
            }
            else if (TryGetEnumReadOperation(ctx, ListReadContext.TempCollectionReadTargetToken, ctx.NameSym, typeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(ListReadContext.TempCollectionToken, CollectionAddToken, Argument(enumOp.TempExpr))));
                goto SECTIONS;
            }
            ReportUnsuporterTypeError(ctx.NameSym, trueTypeArg);
        SECTIONS:
            sections.Add(SwitchSection(SimpleMemberAccess(StateNameToken(ctx.Root), NameOfEnumCollectionStatesToken, InitialEnumStateToken), 
                Block(
                    IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(ListReadContext.DocLenToken))),
                    SimpleAssignExprStatement(CollectionLowStateMemberAccess(ctx), ObjectCreation(ConstructCollectionType(trueType))),
                    AddAssignmentExprStatement(StateConsumedMemberAccess, SizeOfInt32Expr),
                    BreakStatement)));
            sections.Add(SwitchSection(SimpleMemberAccess(StateNameToken(ctx.Root), NameOfEnumCollectionStatesToken, InProgressEnumStateToken),
                Block(
                    builder.ToArray()
                    )));
            sections.Add(SwitchSection(SimpleMemberAccess(StateNameToken(ctx.Root), NameOfEnumCollectionStatesToken, EndMarkerEnumStateToken), 
                Block(
                    IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(Identifier("endMarker")))),
                    IfStatement(
                        BinaryExprNotEquals(Identifier("endMarker"), NumericLiteralExpr((byte)'\x00')),
                        Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(Identifier("endMarker")))))),
                    AddAssignmentExprStatement(StateConsumedMemberAccess, NumericLiteralExpr(1)),
                    ReturnTrueStatement)));
            return SwitchStatement(StateStateAccess, sections);
        }
        private static MethodDeclarationSyntax TryParseListCollectionMethod(MemberContext ctx, ITypeSymbol type)
        {
            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);
            var StateConsumedAssign = AddAssignmentExprStatement(StateConsumedMemberAccess, BinaryExprMinus(ReaderBytesConsumedExpr, StartCheckpointToken));
            //ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if (TryGenerateCollectionTryParseBson(ctx, trueTypeArg, ctx.NameSym, ListReadContext, builder))
            {
                goto RETURN;
            }
            if (TryGenerateCollectionSimpleRead(ctx, trueTypeArg, ListReadContext, builder))
            {
                goto RETURN;
            }

            //if (TryGetEnumReadOperation(ListReadContext.TempCollectionReadTargetToken, ctx.NameSym, trueTypeArg, true, out var enumOp))
            if (TryGetEnumReadOperation(ctx, ListReadContext.TempCollectionReadTargetToken, ctx.NameSym, typeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(ListReadContext.TempCollectionToken, CollectionAddToken, Argument(enumOp.TempExpr))));
                goto RETURN;
            }
            ReportUnsuporterTypeError(ctx.NameSym, trueTypeArg);
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: CollectionTryParseMethodName(type),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 RefParameter(StateDotCollectionStateType(ctx), StateToken),
                                                 OutParameter(SequencePositionType, PositionToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(PositionToken, ReaderPositionExpr),
                       VarLocalDeclarationStatement(StartCheckpointToken, ReaderBytesConsumedExpr),
                       
                       GenerateListStatesSwitch(ctx),
                       //VarLocalDeclarationStatement(ListReadContext.TempCollectionToken, ObjectCreation(ConstructCollectionType(type))),
                       //IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(ListReadContext.DocLenToken))),
                       //VarLocalDeclarationStatement(ListReadContext.UnreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       //VarLocalDeclarationStatement(LocalDocLenToken, SimpleMemberAccess(StateToken, DocLenToken)),
                       //VarLocalDeclarationStatement(LocalCollectionToken, SimpleMemberAccess(StateToken, CollectionToken)),
                       VarLocalDeclarationStatement(SmallConsumedToken, StateConsumedMemberAccess),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprPlus(SmallConsumedToken, LocalConsumedToken),
                                   BinaryExprMinus(ListReadContext.DocLenToken, NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   VarLocalDeclarationStatement(LoopCheckpointToken, ReaderBytesConsumedExpr),
                                   SimpleAssignExprStatement(PositionToken, ReaderPositionExpr),
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(ListReadContext.BsonTypeToken))),
                                   IfNotReturnFalse(TrySkipCStringExpr),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(ListReadContext.BsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(CollectionLowStateMemberAccess(ctx), CollectionAddToken, Argument(DefaultLiteralExpr())),
                                           SimpleAssignExprStatement(LoopCheckpointToken, BinaryExprMinus(ReaderBytesConsumedExpr, StartCheckpointToken)),
                                           ContinueStatement
                                           ))
                                   
                                   )
                               .AddStatements(builder.ToArray())),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(ListReadContext.EndMarkerToken))),
                       IfStatement(
                           BinaryExprNotEquals(ListReadContext.EndMarkerToken, NumericLiteralExpr((byte)'\x00')),
                           Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(ListReadContext.EndMarkerToken))))),
                       SimpleAssignExprStatement(ListReadContext.OutMessageToken, ListReadContext.TempCollectionToken),
                       ReturnTrueStatement
                       ));
        }

        private static MethodDeclarationSyntax WriteListCollectionMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            var index = Identifier("index");
            var array = Identifier("array");
            var loopItem = Identifier("item");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[0];
            var haveCollectionIndexator = HaveCollectionIntIndexator(type);
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            var elementExpr = haveCollectionIndexator
                ? ElementAccessExpr(array, index)
                : IdentifierName(loopItem);

            if (typeArg.IsReferenceType)
            {
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(loopItem, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(loopItem))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.IsValueType)
            {
                var operation = WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, SimpleMemberAccess(loopItem, NullableValueToken));
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(loopItem, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(operation));
            }
            else
            {
                writeOperation.AddRange(WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(loopItem)));
            }

            var loopStatement = haveCollectionIndexator
                ? ForStatement(
                    condition: BinaryExprLessThan(index, SimpleMemberAccess(array, ListCountToken)),
                    incrementor: PostfixUnaryExpr(index),
                    body: Block(VarLocalDeclarationStatement(loopItem, elementExpr), writeOperation!))
                : ForEachStatement(
                    identifier: loopItem,
                    expression: IdentifierName(array),
                    body: Block(writeOperation!, AddAssignmentExpr(index, NumericLiteralExpr(1))));

            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: CollectionWriteMethodName(trueType),
                    parameterList: ParameterList(
                        RefParameter(BsonWriterType, BsonWriterToken),
                        Parameter(TypeFullName(trueType), array)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                Block(
                    LocalDeclarationStatement(IntPredefinedType(), index, NumericLiteralExpr(0)),
                    VarLocalDeclarationStatement(checkpoint, WriterWrittenExpr),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)),
                    loopStatement,
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWrittenExpr, IdentifierName(checkpoint))),
                    Statement(ReservedWrite(reserved, docLength)),
                    Statement(WriterCommitExpr)
                ));
        }
    }

}
