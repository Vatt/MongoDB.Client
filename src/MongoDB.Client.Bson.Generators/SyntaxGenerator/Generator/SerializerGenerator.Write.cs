using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken WriteArrayMethodName(MemberContext ctx, ITypeSymbol typeSymbol)
        {
            var name = $"Write{ctx.NameSym.Name}{typeSymbol.Name}";/*{typeSymbol.GetTypeMembers()[0].Name}*/
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
        private static MethodDeclarationSyntax[] GenerateWriteArrayMethods(ClassContext ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members)
            {
                if (member.TypeSym.ToString().Contains("System.Collections.Generic.List") ||
                    member.TypeSym.ToString().Contains("System.Collections.Generic.IList"))
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
                        if (type is null || type.TypeArguments.IsEmpty)
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static MethodDeclarationSyntax WriteArrayMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol trueType = type.Name.Equals("Nullable") ? ((INamedTypeSymbol)type).TypeParameters[0] : type;
            var classCtx = ctx.Root;
            var checkpoint = SF.Identifier("checkpoint");
            var reserved = SF.Identifier("reserved");
            var docLength = SF.Identifier("docLength");
            var sizeSpan = SF.Identifier("sizeSpan");
            var index = SF.Identifier("index");
            var array = SF.Identifier("array");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[0];
            StatementSyntax writeOperation = default;
            if (typeArg.IsReferenceType)
            {
                writeOperation =
                    SF.IfStatement(
                        condition: BinaryExprEqualsEquals(ElementAccessExpr(IdentifierName(array), index), NullLiteralExpr()),
                        statement: SF.Block(Statement(WriteBsonNull(index))),
                        @else: SF.ElseClause(SF.Block(WriteOperation(ctx, index, typeArg, classCtx.BsonWriterId, ElementAccessExpr(IdentifierName(array), index)))));
            }
            else
            {
                writeOperation = WriteOperation(ctx, index, typeArg, classCtx.BsonWriterId, ElementAccessExpr(IdentifierName(array), index));
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
                .WithBody(SF.Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)),
                    ForStatement(
                        indexVar: index,
                        condition: BinaryExprLessThan(IdentifierName(index), SimpleMemberAccess(IdentifierName(array), IdentifierName("Count"))),
                        incrementor: PostfixUnaryExpr(index),
                        body: SF.Block(writeOperation!)),
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), IdentifierName(checkpoint))),
                    LocalDeclarationStatement(SpanByte(), sizeSpan, StackAllocByteArray(4)),
                    Statement(BinaryPrimitivesWriteInt32LittleEndian(sizeSpan, docLength)),
                    Statement(ReservedWrite(reserved, sizeSpan)),
                    Statement(WriterCommit())
                ));
        }
        private static MethodDeclarationSyntax WriteMethod(ClassContext ctx)
        {
            var decl = ctx.Declaration;

            var body = ctx.Declaration.TypeKind == TypeKind.Enum ? WriteEnumBody(ctx) : WriteDefaultBody(ctx);
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    explicitInterfaceSpecifier: SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: VoidPredefinedType(),
                    identifier: SF.Identifier("Write"),
                    parameterList: ParameterList(
                        RefParameter(ctx.BsonWriterType, ctx.BsonWriterToken),
                        InParameter(TypeFullName(ctx.Declaration), ctx.TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(body);
        }

        private static BlockSyntax WriteEnumBody(ClassContext ctx)
        {
            var repr = AttributeHelper.GetEnumRepresentation(ctx.Declaration);
            return (repr) switch
            {
                1 => StringRepresentation(ctx),
                2 => Int32Representation(ctx),
                3 => Int64Representation(ctx)
            };
            static BlockSyntax StringRepresentation(ClassContext ctx)
            {
                List<StatementSyntax> statements = new();
                foreach (var member in ctx.Members)
                {
                    var sma = SimpleMemberAccess(IdentifierFullName(member.TypeSym), IdentifierName(member.NameSym));
                    statements.Add(
                        SF.IfStatement(
                            condition: BinaryExprEqualsEquals(ctx.WriterInputVar, sma),
                            statement: SF.Block(Statement(WriteString(StaticFieldNameToken(member))))
                        ));
                }
                return SF.Block(statements.ToArray());
            }
            static BlockSyntax Int32Representation(ClassContext ctx)
            {
                return SF.Block
                (
                    Statement(WriteInt32(CastToInt(ctx.WriterInputVar)))
                );
            }
            static BlockSyntax Int64Representation(ClassContext ctx)
            {
                return SF.Block
                (
                    Statement(WriteInt64(CastToLong(ctx.WriterInputVar)))
                );
            }
        }
        private static BlockSyntax WriteDefaultBody(ClassContext ctx)
        {
            var checkpoint = SF.Identifier("checkpoint");
            var reserved = SF.Identifier("reserved");
            var docLength = SF.Identifier("docLength");
            var sizeSpan = SF.Identifier("sizeSpan");
            List<StatementSyntax> statements = new();

            foreach (var member in ctx.Members)
            {
                var writeTarget = SimpleMemberAccess(ctx.WriterInputVar, IdentifierName(member.NameSym));
                ITypeSymbol trueType = member.TypeSym.Name.Equals("Nullable") ? ((INamedTypeSymbol)member.TypeSym).TypeParameters[0] : member.TypeSym;
                if (trueType is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
                {
                    /*if (namedType.ToString().Contains("System.Collections.Generic.List") ||
                        namedType.ToString().Contains("System.Collections.Generic.IList"))
                    {
                        statements.Add(Statement(Write_Type_Name(4, IdentifierName(StaticFieldNameToken(member)))));
                    }
                    */

                }

                // statements.Add(
                //     SF.IfStatement(
                //         condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                //         statement: SF.Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                //         @else:SF.ElseClause(SF.Block(WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget)))));
                _ = AttributeHelper.TryGetBsonWriteIgnoreIfAttr(member, out var condition);
                if (member.TypeSym.IsReferenceType)
                {
                    if (condition != null)
                    {
                        statements.Add(
                            IfNot(
                                condition,
                                SF.IfStatement(
                                    condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                    statement: SF.Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                                    @else: SF.ElseClause(SF.Block(WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget)))))
                            );
                    }
                    else
                    {
                        statements.Add(
                             SF.IfStatement(
                                 condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                 statement: SF.Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                                 @else: SF.ElseClause(SF.Block(WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget)))));
                    }
 
                }
                else
                {
                    statements.Add(WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget));
                }


            }

            return SF.Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)))
                .AddStatements(
                    statements.ToArray())
                .AddStatements(
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), IdentifierName(checkpoint))),
                    LocalDeclarationStatement(SpanByte(), sizeSpan, StackAllocByteArray(4)),
                    Statement(BinaryPrimitivesWriteInt32LittleEndian(sizeSpan, docLength)),
                    Statement(ReservedWrite(reserved, sizeSpan)),
                    Statement(WriterCommit())
                    );
        }

        public static StatementSyntax WriteOperation(MemberContext ctx, SyntaxToken name, ITypeSymbol typeSym,
            ExpressionSyntax writerId, ExpressionSyntax writeTarget)
        {
            ITypeSymbol trueType = typeSym.Name.Equals("Nullable") ? ((INamedTypeSymbol)typeSym).TypeParameters[0] : typeSym;
            if (TryGetSimpleWriteOperation(trueType, name, writeTarget, out var expr))
            {

                return SF.ExpressionStatement(expr);
            }
            if (ctx.Root.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueType.Name)) != default)
            {
                return SF.Block(
                    VarLocalDeclarationStatement(SF.Identifier("genericReserved"), WriterReserve(1)),
                    Statement(WriteCString(StaticFieldNameToken(ctx))),
                    Statement(WriteGeneric(writeTarget, SF.IdentifierName("genericReserved"))));
            }
            if (trueType is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (namedType.ToString().Contains("System.Collections.Generic.List") ||
                    namedType.ToString().Contains("System.Collections.Generic.IList"))
                {
                    return SF.Block(
                        Statement(Write_Type_Name(4, IdentifierName(name))),
                        InvocationExprStatement(IdentifierName(WriteArrayMethodName(ctx, trueType)), RefArgument(writerId), Argument(writeTarget)));
                }
            }
            else
            {
                foreach (var context in ctx.Root.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.ToString().Equals(trueType.ToString()))
                    {
                        if (ctx.TypeSym.TypeKind == TypeKind.Enum)
                        {
                            //TODO: Удалить чисельные репрезентаци, оставить только строковые
                            return SF.Block(
                                Statement(Write_Type_Name(2, IdentifierName(StaticFieldNameToken(ctx)))),
                                Statement(GeneratedSerializerWrite(context, writerId, writeTarget)));
                        }
                        return SF.Block(
                            Statement(Write_Type_Name(3, IdentifierName(StaticFieldNameToken(ctx)))),
                            Statement(GeneratedSerializerWrite(context, writerId, writeTarget)));
                    }
                }
            }
            return SF.ReturnStatement();
        }
        private static bool TryGetSimpleWriteOperation(ITypeSymbol typeSymbol, SyntaxToken bsonNameToken,
            ExpressionSyntax writeTarget, out InvocationExpressionSyntax expr)
        {
            expr = default;
            var bsonName = IdentifierName(bsonNameToken);
            switch (typeSymbol.ToString())
            {
                case "double":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "string":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonDocument":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonArray":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonObjectId":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "bool":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "int":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "long":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "System.Guid":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                case "System.DateTimeOffset":
                    expr = Write_Type_Name_Value(bsonName, writeTarget);
                    return true;
                default:
                    return false;
            }
        }
    }
}