using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken WriteArrayMethodName(MemberContext ctx, ITypeSymbol typeSymbol)
        {
            var name = $"Write{ctx.NameSym.Name}{typeSymbol.Name}";
            var type = typeSymbol as INamedTypeSymbol;

            while (true)
            {
                name = $"{name}{type.TypeArguments[0].Name}";
                type = type.TypeArguments[0] as INamedTypeSymbol;
                if (type is null || type.TypeArguments.IsEmpty)
                {
                    break;
                }
            }
            return SF.Identifier(name);
        }
        private static SyntaxToken ReadArrayMethodName(ISymbol nameSym, ITypeSymbol typeSymbol)
        {
            var name = $"TryParse{nameSym.Name}{typeSymbol.Name}";
            var type = typeSymbol as INamedTypeSymbol;

            while (true)
            {
                name = $"{name}{type.TypeArguments[0].Name}";
                type = type.TypeArguments[0] as INamedTypeSymbol;
                if (type is null || type.TypeArguments.IsEmpty)
                {
                    break;
                }
            }
            return SF.Identifier(name);
        }
        private static MethodDeclarationSyntax[] GenerateReadArrayMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members)
            {
                if (TypeLib.IsListOrIList(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(ReadArrayMethod(member, member.TypeSym));
                        break;
                    }
                    // if (type.TypeArguments[0] is INamedTypeSymbol arg1 && arg1.TypeArguments.Length == 0)
                    // {
                    //     methods.Add(ReadArrayMethod(member, type));
                    //     continue;
                    // }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(ReadArrayMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        //if (type is null || type.TypeArguments.IsEmpty)
                        if (type is null || (TypeLib.IsListOrIList(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static MethodDeclarationSyntax[] GenerateWriteArrayMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            foreach (var member in ctx.Members)
            {
                if (TypeLib.IsListOrIList(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(WriteArrayMethod(member, member.TypeSym));
                        break;
                    }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(WriteArrayMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        //if (type is null || type.TypeArguments.IsEmpty)
                        if (type is null || (TypeLib.IsListOrIList(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static bool TryGenerateSimpleReadOpereation(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken bsonType, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, ctx.Root.BsonReaderId, TypedVariableDeclarationExpr(TypeFullName(type), readTarget), bsonType);
            if(operation == default)
            {
                return false;
            }
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else:
                                            Block(
                                                InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(tempVar.HasValue ? tempVar.Value : readTarget)),
                                                ContinueStatement()));
            return true;
        }
        private static bool TryGenerateTryParseBsonArrayOperation(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            if(IsBsonSerializable(type) == false)
            {
                return false;
            }
            var operation = InvocationExpr(IdentifierName(type.ToString()), IdentifierName("TryParseBson"), RefArgument(ctx.Root.BsonReaderId), OutArgument(TypedVariableDeclarationExpr(TypeFullName(type), readTarget)));
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else: Block(InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(readTarget)), ContinueStatement()));
            return true;
        }
        private static bool TryGenerateArrayReadEnumOperation(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            if(type.TypeKind != TypeKind.Enum)
            {
                return false;
            }
            var localReadEnumVar = Identifier($"{readTarget}EnumTemp");
            int repr = GetEnumRepresentation(ctx.NameSym);
            if (repr == -1) { repr = 2; }
            if (repr != 1)
            {
                builder.Statements(
                          repr == 2 ?
                            LocalDeclarationStatement(IntPredefinedType(), localReadEnumVar, DefaultLiteralExpr()) :
                            LocalDeclarationStatement(LongPredefinedType(), localReadEnumVar, DefaultLiteralExpr()),
                          repr == 2 ?     
                            IfNotReturnFalseElse(TryGetInt32(localReadEnumVar), Block(InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(Cast(type, localReadEnumVar))))) :
                            IfNotReturnFalseElse(TryGetInt64(localReadEnumVar), Block(InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(Cast(type, localReadEnumVar)))))
                    );
            }
            else
            {
                //var readMethod = IdentifierName(ReadStringReprEnumMethodName(member.Root, member.TypeMetadata, member.NameSym));
                var readMethod = IdentifierName(ReadStringReprEnumMethodName(ctx.Root, type, ctx.NameSym));
                builder.IfNotReturnFalseElse(condition: InvocationExpr(readMethod, RefArgument(ctx.Root.BsonReaderToken), OutArgument(VarVariableDeclarationExpr(localReadEnumVar))), 
                                             @else: Block(InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(localReadEnumVar))));
            }
            return true;
        }
        private static MethodDeclarationSyntax ReadArrayMethod(MemberContext ctx, ITypeSymbol type)
        {
            var docLenToken = Identifier("arrayDocLength");
            var unreadedToken = Identifier("arrayUnreaded");
            var endMarkerToken = Identifier("arrayEndMarker");
            var bsonTypeToken = Identifier("arrayBsonType");
            var bsonNameToken = Identifier("arrayBsonName");
            var outMessage = Identifier("array");
            var tempArrayRead = Identifier("temp");

            var typeArg = ExtractTypeFromNullableIfNeed((type as INamedTypeSymbol).TypeArguments[0]);

            var trueType = ExtractTypeFromNullableIfNeed(type);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if(TryGenerateSimpleReadOpereation(ctx, typeArg, tempArrayRead, bsonTypeToken, outMessage, builder))
            {
                goto RETURN;
            }
            if(TryGenerateTryParseBsonArrayOperation(ctx, typeArg, tempArrayRead, outMessage, builder))
            {
                goto RETURN;
            }
            if(TryGenerateArrayReadEnumOperation(ctx, typeArg, tempArrayRead, outMessage, builder))
            {
                goto RETURN;
            }
            //TODO: Сгенерировать SerializersMap если ниодин не отработал
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: ReadArrayMethodName(ctx.NameSym, type),
                    parameterList: ParameterList(RefParameter(ctx.Root.BsonReaderType, ctx.Root.BsonReaderToken),
                                                 OutParameter(IdentifierName(type.ToString()), outMessage)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(IdentifierName(outMessage), ObjectCreation(TypeFullName(trueType))),
                       IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                       VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                                   BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                                   IfNotReturnFalse(TrySkipCString()),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(bsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(outMessage, IdentifierName("Add"), Argument(DefaultLiteralExpr())),
                                           ContinueStatement()
                                           )),
                                   builder.ToArray()
                                   )),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                       IfStatement(
                           BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                           Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(endMarkerToken))))),
                       ReturnStatement(TrueLiteralExpr())
                       ));
        }

        private static MethodDeclarationSyntax WriteArrayMethod(MemberContext ctx, ITypeSymbol type)
        {
            //ITypeSymbol trueType = type.Name.Equals("Nullable") ? ((INamedTypeSymbol)type).TypeArguments[0] : type;
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var classCtx = ctx.Root;
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            var index = Identifier("index");
            var array = Identifier("array");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[0];
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            if (typeArg.IsReferenceType)
            {
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(ElementAccessExpr(array, index), NullLiteralExpr()),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(WriteOperation(ctx, index, ctx.NameSym, typeArg, classCtx.BsonWriterId, ElementAccessExpr(array, index))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.TypeKind == TypeKind.Struct)
            {
                var operation = WriteOperation(ctx, index, ctx.NameSym, typeArg, classCtx.BsonWriterId, SimpleMemberAccess(ElementAccessExpr(IdentifierName(array), index), IdentifierName("Value")));
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(ElementAccessExpr(array, index), IdentifierName("HasValue")), FalseLiteralExpr()),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(operation));
            }
            else
            {
                writeOperation.AddRange(WriteOperation(ctx, index, ctx.NameSym, typeArg, classCtx.BsonWriterId, ElementAccessExpr(IdentifierName(array), index)));
            }

            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: WriteArrayMethodName(ctx, trueType),
                    parameterList: ParameterList(
                        RefParameter(classCtx.BsonWriterType, classCtx.BsonWriterToken),
                        Parameter(TypeFullName(trueType), array)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)),
                    ForStatement(
                        indexVar: index,
                        condition: BinaryExprLessThan(index, SimpleMemberAccess(array, IdentifierName("Count"))),
                        incrementor: PostfixUnaryExpr(index),
                        body: Block(writeOperation!)),
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), IdentifierName(checkpoint))),
                    LocalDeclarationStatement(SpanByte(), sizeSpan, StackAllocByteArray(4)),
                    Statement(BinaryPrimitivesWriteInt32LittleEndian(sizeSpan, docLength)),
                    Statement(ReservedWrite(reserved, sizeSpan)),
                    Statement(WriterCommit())
                ));
        }
    }

}