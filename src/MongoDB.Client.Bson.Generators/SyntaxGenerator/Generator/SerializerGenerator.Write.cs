using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    identifier: WriteBsonToken,
                    parameterList: ParameterList(
                        RefParameter(BsonWriterType, BsonWriterToken),
                        InParameter(TypeFullName(ctx.Declaration), TryParseOutVarToken)),
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
                var writeTarget = SimpleMemberAccess(WriterInputVarToken, IdentifierName(member.NameSym));
                ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                TryGetBsonWriteIgnoreIfAttr(member, out var condition);

                if (member.BsonElementAlias.Equals("_id", StringComparison.InvariantCulture) && IsBsonObjectId(trueType) && member.IsReadOnly == false)
                {
                    inner.IfStatement(
                            BinaryExprEqualsEquals(writeTarget, Default(TypeFullName(trueType))),
                            Block(SimpleAssignExprStatement(writeTarget, NewBsonObjectIdExpr)));
                }

                if (trueType.IsReferenceType)
                {
                    inner.IfStatement(
                                condition: BinaryExprEqualsEquals(writeTarget, NullLiteralExpr()),
                                statement: Block(Statement(WriteBsonNull(member.StaticSpanNameToken))),
                                @else: Block(WriteOperation(member, member.StaticSpanNameToken, member.NameSym, member.TypeSym, BsonWriterToken, writeTarget)));
                }
                else if (member.TypeSym.NullableAnnotation == NullableAnnotation.Annotated && trueType.IsValueType == true)
                {
                    var nullableStrcutTarget = SimpleMemberAccess(writeTarget, NullableValueToken);
                    inner.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(writeTarget, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(member.StaticSpanNameToken)),
                            @else: Block(WriteOperation(member, member.StaticSpanNameToken, member.NameSym, member.TypeSym, BsonWriterToken, nullableStrcutTarget)));
                }
                else
                {
                    inner.AddRange(WriteOperation(member, member.StaticSpanNameToken, member.NameSym, member.TypeSym, BsonWriterToken, writeTarget));
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
                    VarLocalDeclarationStatement(checkpoint, WriterWrittenExpr),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)))
                .AddStatements(
                    statements.ToArray())
                .AddStatements(
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWrittenExpr, checkpoint)),
                    Statement(ReservedWrite(reserved, docLength)),
                    Statement(WriterCommitExpr)
                    );
        }
        public static StatementSyntax[] WriteOperation(MemberContext ctx, SyntaxToken name, ISymbol nameSym, ITypeSymbol typeSym, SyntaxToken writerId, ExpressionSyntax writeTarget)
        {
            var trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (TryGenerateBsonWrite(name, nameSym, typeSym, writeTarget, out var bsonWriteExpr))
            {
                return bsonWriteExpr.ToArray();
            }
            if (TryGenerateSimpleWriteOperation(nameSym, trueType, name, writeTarget, out var expr))
            {

                return Statements(expr);
            }
            if (TryGenerateWriteEnum(ctx, typeSym, name, writeTarget, out var enumStatements))
            {
                return enumStatements.ToStatements().ToArray();
            }
            if (ctx.Root.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueType.Name)) != default)
            {
                var identifierName = $"{name}genericReserved";
                return Statements
                (
                    VarLocalDeclarationStatement(Identifier(identifierName), WriterReserve(1)),
                    Statement(WriteName(name)),
                    Statement(WriteGeneric(writeTarget, IdentifierName(identifierName)))
                );
            }

            if (typeSym.SpecialType == SpecialType.System_Object)
            {
                var identifierName = $"{name}objectReserved";
                return Statements
                (
                    VarLocalDeclarationStatement(Identifier(identifierName), WriterReserve(1)),
                    Statement(WriteName(name)),
                    Statement(WriteObject(writeTarget, IdentifierName(identifierName)))
                );
            }
            if (trueType is INamedTypeSymbol namedType)
            {
                if (IsListCollection(namedType))
                {
                    return Statements
                    (
                        Statement(Write_Type_Name(4, name)),
                        //InvocationExprStatement(CollectionWriteMethodName(typeSym), RefArgument(writerId), Argument(writeTarget))
                        InvocationExprStatement(CollectionWriteMethodName(trueType), RefArgument(writerId), Argument(writeTarget))
                    );
                }

                if (IsDictionaryCollection(namedType))
                {
                    return Statements
                    (
                        Statement(Write_Type_Name(3, name)),
                        //InvocationExprStatement(CollectionWriteMethodName(typeSym), RefArgument(writerId), Argument(writeTarget))
                        InvocationExprStatement(CollectionWriteMethodName(trueType), RefArgument(writerId), Argument(writeTarget))
                    );
                }


            }
            ReportUnsuporterTypeError(ctx.NameSym, ctx.TypeSym);
            return new StatementSyntax[0];
        }
        public static bool TryGenerateWriteEnum(MemberContext member, ITypeSymbol typeSym, SyntaxToken bsonName, ExpressionSyntax writeTarget, out ImmutableList<ExpressionSyntax> statements)
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
                statements = ImmutableList.Create(Write_Type_Name_Value(bsonName, repr == 2 ? CastToInt(writeTarget) : CastToLong(writeTarget)));
            }
            else
            {
                var methodName = IdentifierName(WriteStringReprEnumMethodName(trueType, member.NameSym));
                statements = ImmutableList.Create(
                    Write_Type_Name(2, bsonName),
                    InvocationExpr(methodName, RefArgument(BsonWriterToken), /*Argument(bsonName),*/ Argument(writeTarget)));
            }
            return true;
        }
        public static int reservedCnt = 0;
        public static int bsonTypeCnt = 0;
        public static bool TryGenerateBsonWrite(SyntaxToken nameToken, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax writeTarget, out ImmutableList<StatementSyntax> expressions)
        {
            expressions = default;
            var bsonTypeToken = Identifier($"bsonTypeTemp{bsonTypeCnt++}");
            var bsonReserved = Identifier($"bsonTypeResereved{reservedCnt++}");
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (IsBsonSerializable(trueType))
            {
                expressions = ImmutableList.Create(
                    Statement(Write_Type_Name(3, nameToken)),
                    InvocationExprStatement(IdentifierName(trueType.ToString()), WriteBsonToken, RefArgument(BsonWriterToken), Argument(writeTarget)));
                return true;
            }
            if (IsBsonExtensionSerializable(nameSym, trueType, out var extSym))
            {
                expressions = ImmutableList.Create(
                    LocalDeclarationStatement(VarType, bsonReserved, WriterReserve(1)),
                    Statement(WriteName(nameToken)),
                    InvocationExprStatement(IdentifierName(extSym.ToString()), WriteBsonToken, RefArgument(BsonWriterToken), Argument(writeTarget), OutArgument(VarVariableDeclarationExpr(bsonTypeToken))),
                    Statement(ReservedWriteByte(bsonReserved, bsonTypeToken)));
                return true;
            }

            return false;

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
            if (IsArrayByteOrMemoryByte(typeSymbol))
            {
                var arrayRepr = GetBinaryDataRepresentation(nameSym);
                arrayRepr = arrayRepr == -1 ? 0 : arrayRepr;
                switch (arrayRepr)
                {
                    case 0: break;
                    case 5: break;
                    default:
                        ReportUnsuportedByteArrayReprError(nameSym, typeSymbol);
                        break;
                }
                expr = Write_Type_Name_Value(bsonName, arrayRepr, writeTarget);
                return true;
            }
            if (IsBsonTimestamp(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (IsBsonDocument(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (IsBsonArray(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (IsBsonObjectId(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (IsGuid(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            if (IsDateTimeOffset(typeSymbol))
            {
                expr = Write_Type_Name_Value(bsonName, writeTarget);
                return true;
            }
            //if (typeSymbol.SpecialType != SpecialType.None)
            //{
            //    ReportUnsuporterTypeError(nameSym, typeSymbol);
            //}
            return false;
        }
    }
}
