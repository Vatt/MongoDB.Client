using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    modifiers: default,
                    explicitInterfaceSpecifier: SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(ctx.Declaration))),
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
                .WithBody(WriteMethodBody(ctx));
        }
        public static StatementSyntax GenerateWriteEnum(ContextCore ctx, MemberContext member, ExpressionSyntax writeTarget)
        {
            int repr = AttributeHelper.GetEnumRepresentation(member.NameSym);
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
            var checkpoint = SF.Identifier("checkpoint");
            var reserved = SF.Identifier("reserved");
            var docLength = SF.Identifier("docLength");
            var sizeSpan = SF.Identifier("sizeSpan");
            List<StatementSyntax> statements = new();

            foreach (var member in ctx.Members)
            {
                StatementSyntax statement;
                var writeTarget = SimpleMemberAccess(ctx.WriterInputVar, IdentifierName(member.NameSym));
                ITypeSymbol trueType = member.TypeSym.Name.Equals("Nullable") ? ((INamedTypeSymbol)member.TypeSym).TypeParameters[0] : member.TypeSym;
                AttributeHelper.TryGetBsonWriteIgnoreIfAttr(member, out var condition);
                if (member.TypeSym.TypeKind == TypeKind.Enum)
                {
                    statement = GenerateWriteEnum(ctx, member, writeTarget);
                    goto CONDITION_CHECK;
                }
                if (member.TypeSym.IsReferenceType)
                {
                    statement = SF.IfStatement(
                                    condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                    statement: SF.Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                                    @else: SF.ElseClause(SF.Block(WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget))));
                }
                else
                {
                    if (member.BsonElementAlias.Equals("_id") && TypeLib.IsBsonOBjectId(member.TypeSym))
                    {
                        statements.Add(
                            SF.IfStatement(
                                BinaryExprEqualsEquals(writeTarget, Default(TypeFullName(member.TypeSym))),
                                SF.Block(SimpleAssignExprStatement(writeTarget, NewBsonObjectId()))));
                    }
                    statement = WriteOperation(member, StaticFieldNameToken(member), member.TypeSym, ctx.BsonWriterId, writeTarget);
                }
CONDITION_CHECK:
                if (condition != null)
                {
                    statements.Add(IfNot(condition, SF.Block(statement)));
                }
                else
                {
                    statements.Add(statement);
                }
            }

            return SF.Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)))
                .AddStatements(
                    statements.ToArray())
                .AddStatements(
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), checkpoint)),
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
                if (TypeLib.IsListOrIList(namedType))
                {
                    return SF.Block(
                        Statement(Write_Type_Name(4, name)),
                        InvocationExprStatement(WriteArrayMethodName(ctx, trueType), RefArgument(writerId), Argument(writeTarget)));
                }
            }
            else
            {
                foreach (var context in ctx.Root.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.ToString().Equals(trueType.ToString()))
                    {
                        return SF.Block(
                            Statement(Write_Type_Name(3, StaticFieldNameToken(ctx))),
                            Statement(GeneratedSerializerWrite(context, writerId, writeTarget)));
                    }
                }
            }
            return default;
        }
        private static bool TryGetSimpleWriteOperation(ITypeSymbol typeSymbol, SyntaxToken bsonNameToken, ExpressionSyntax writeTarget, out InvocationExpressionSyntax expr)
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
            if(typeSymbol.Equals(TypeLib.BsonDocument, SymbolEqualityComparer.Default))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (typeSymbol.Equals(TypeLib.BsonArray, SymbolEqualityComparer.Default))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (typeSymbol.Equals(TypeLib.BsonObjectId, SymbolEqualityComparer.Default))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (typeSymbol.Equals(TypeLib.System_Guid, SymbolEqualityComparer.Default))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (typeSymbol.Equals(TypeLib.System_DateTimeOffset, SymbolEqualityComparer.Default))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            return false;
        }
    }
}
