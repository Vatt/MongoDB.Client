using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using System.Collections.Immutable;
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
        private static BlockSyntax WriteMethodBody(ContextCore ctx)
        {
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            var statements = ImmutableList.CreateBuilder<StatementSyntax>();

            foreach (var member in ctx.Members)
            {
                var inner = ImmutableList.CreateBuilder<StatementSyntax>();
                var writeTarget = SimpleMemberAccess(ctx.WriterInputVar, IdentifierName(member.NameSym));
                ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                TryGetBsonWriteIgnoreIfAttr(member, out var condition);

                if (member.BsonElementAlias.Equals("_id") && TypeLib.IsBsonObjectId(trueType))
                {
                    inner.IfStatement(
                            BinaryExprEqualsEquals(writeTarget, Default(TypeFullName(trueType))),
                            Block(SimpleAssignExprStatement(writeTarget, NewBsonObjectId)));
                }

                if (trueType.IsReferenceType)
                {
                    inner.IfStatement(
                                condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                statement: Block(Statement(WriteBsonNull(StaticFieldNameToken(member)))),
                                @else: Block(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, member.TypeSym, ctx.BsonWriterId, writeTarget)));
                }
                //else if (member.TypeSym.NullableAnnotation == NullableAnnotation.Annotated && trueType.TypeKind == TypeKind.Struct)
                else if (member.TypeSym.NullableAnnotation == NullableAnnotation.Annotated && trueType.IsValueType == true)
                {
                    var nullableStrcutTarget = SimpleMemberAccess(writeTarget, IdentifierName("Value"));
                    inner.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(writeTarget, IdentifierName("HasValue")), FalseLiteralExpr()),
                            statement: Block(WriteBsonNull(StaticFieldNameToken(member))),
                            @else: Block(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, member.TypeSym, ctx.BsonWriterId, nullableStrcutTarget)));
                }
                else
                {
                    inner.AddRange(WriteOperation(member, StaticFieldNameToken(member), member.NameSym, member.TypeSym, ctx.BsonWriterId, writeTarget));
                }
                if (condition != null)
                {
                    statements.IfNot(condition, Block(inner));
                }
                else
                {
                    statements.AddRange(inner);
                }
            }

            return Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWritten()),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)))
                .AddStatements(
                    statements.ToArray())
                .AddStatements(
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWritten(), checkpoint)),
                    LocalDeclarationStatement(SpanByte, sizeSpan, StackAllocByteArray(4)),
                    Statement(BinaryPrimitivesWriteInt32LittleEndian(sizeSpan, docLength)),
                    Statement(ReservedWrite(reserved, sizeSpan)),
                    Statement(WriterCommit())
                    );
        }
        public static StatementSyntax[] WriteOperation(MemberContext ctx, SyntaxToken name, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax writerId, ExpressionSyntax writeTarget)
        {
            var trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (TryGenerateSimpleWriteOperation(nameSym, trueType, name, writeTarget, out var expr))
            {

                return Statements(expr);
            }
            if (TryGenerateWriteEnum(ctx.Root, ctx, typeSym, writeTarget, out var enumStatements))
            {
                return enumStatements.ToStatements().ToArray();
            }
            if (ctx.Root.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueType.Name)) != default)
            {
                return Statements
                (
                    VarLocalDeclarationStatement(Identifier($"{name}genericReserved"), WriterReserve(1)),
                    Statement(WriteCString(StaticFieldNameToken(ctx))),
                    Statement(WriteGeneric(writeTarget, IdentifierName($"{name}genericReserved")))
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
            if (TryGenerateBsonWrite(ctx, typeSym, writeTarget, out var bsonWriteExpr))
            {
                return bsonWriteExpr.ToStatements().ToArray();
            }
            GeneratorDiagnostics.ReportSerializerMapUsingWarning(ctx.NameSym);
            return Statements(
                    Statement(Write_Type_Name(3, StaticFieldNameToken(ctx))),
                    OtherWriteBson(ctx)
                );
        }
        public static bool TryGenerateWriteEnum(ContextCore ctx, MemberContext member, ITypeSymbol typeSym, ExpressionSyntax writeTarget, out ImmutableList<ExpressionSyntax> statements)
        {
            statements = default;
            var trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (trueType.TypeKind != TypeKind.Enum)
            {
                return false;
            }
            int repr = GetEnumRepresentation(member.NameSym);
            if (repr == -1) { repr = 2; }
            if (repr != 1)
            {
                statements = ImmutableList.Create(Write_Type_Name_Value(StaticFieldNameToken(member), repr == 2 ? CastToInt(writeTarget) : CastToLong(writeTarget)));
            }
            else
            {
                //var methodName = IdentifierName(WriteStringReprEnumMethodName(ctx, member.TypeMetadata, member.NameSym));
                var methodName = IdentifierName(WriteStringReprEnumMethodName(ctx, trueType, member.NameSym));
                statements = ImmutableList.Create(InvocationExpr(methodName, RefArgument(ctx.BsonWriterToken), Argument(StaticFieldNameToken(member)), Argument(writeTarget)));
            }
            return true;
        }
        public static bool TryGenerateBsonWrite(MemberContext ctx, ITypeSymbol typeSym, ExpressionSyntax writeTarget, out ImmutableList<ExpressionSyntax> expressions)
        {
            expressions = default;
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(typeSym);
            ExpressionSyntax writerId = ctx.Root.BsonWriterId;
            if (IsBsonSerializable(trueType) == false)
            {
                return false;
            }
            expressions = ImmutableList.Create(
                Write_Type_Name(3, StaticFieldNameToken(ctx)),
                InvocationExpr(IdentifierName(trueType.ToString()), IdentifierName("WriteBson"), RefArgument(writerId), Argument(writeTarget)));
            return true;

        }
        private static bool TryGenerateSimpleWriteOperation(ISymbol nameSym, ITypeSymbol typeSymbol, SyntaxToken bsonNameToken, ExpressionSyntax writeTarget, out ExpressionSyntax expr)
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
