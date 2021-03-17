using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private readonly struct ReadOperationContext
        {
            public ExpressionSyntax Expr { get; }
            public ExpressionSyntax TempExpr { get; }
            public ReadOperationContext(ExpressionSyntax expr)
            {
                Expr = expr;
                TempExpr = null;
            }
            public ReadOperationContext(ExpressionSyntax expr, ExpressionSyntax tempExpr)
            {
                Expr = expr;
                TempExpr = tempExpr;
            }
            public void Deconstruct(out ExpressionSyntax expr, out ExpressionSyntax tempExpr)
            {
                expr = Expr;
                tempExpr = TempExpr;
            }
            public static implicit operator ReadOperationContext(ExpressionSyntax expr) => new ReadOperationContext(expr);
        }
        private static MethodDeclarationSyntax TryParseMethod(ContextCore ctx)
        {
            var declaration = ctx.Declaration;
            var docLenToken = Identifier("docLength");
            var unreadedToken = Identifier("unreaded");
            var endMarkerToken = Identifier("endMarker");
            var bsonTypeToken = Identifier("bsonType");
            var bsonNameToken = Identifier("bsonName");
            StatementSyntax[] operations = default;
            switch (ctx.GeneratorMode)
            {
                case 1:
                    operations = ContextTreeTryParseOperations(ctx, bsonTypeToken, bsonNameToken);
                    break;
                case 2:
                    operations = Operations(ctx, bsonTypeToken, bsonNameToken);
                    break;
            }
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: new(PublicKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,//SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: TryParseBsonToken,
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(TypeFullName(declaration), TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                    Block(
                     SimpleAssignExprStatement(TryParseOutVarToken, DefaultLiteralExpr()),
                     DeclareTempVariables(ctx),
                     IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                     VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       SF.WhileStatement(
                           condition:
                              BinaryExprLessThan(
                                  BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemainingExpr),
                                  BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                           statement:
                              Block(
                                  IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                                  IfNotReturnFalse(TryGetCStringAsSpan(VarVariableDeclarationExpr(bsonNameToken))),
                                  IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonTypeToken), NumericLiteralExpr(10))),
                                  operations,
                                  IfNotReturnFalse(TrySkip(IdentifierName(bsonTypeToken))))),
                          IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                          SF.IfStatement(
                              condition:
                                  BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                              statement: Block(SerializerEndMarkerException(declaration, IdentifierName(endMarkerToken)))))
                        .AddStatements(CreateMessage(ctx))
                        .AddStatements(ReturnTrueStatement));
        }
        private static StatementSyntax[] DeclareTempVariables(ContextCore ctx)
        {
            ImmutableList<StatementSyntax>.Builder variables = ImmutableList.CreateBuilder<StatementSyntax>();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (trueType.IsReferenceType)
                {
                    variables.DefaultLocalDeclarationStatement(SF.ParseTypeName(trueType.ToString()), member.AssignedVariableToken);
                }
                else
                {
                    variables.DefaultLocalDeclarationStatement(SF.ParseTypeName(member.TypeSym.ToString()), member.AssignedVariableToken);
                }

            }
            return variables.ToArray();
        }
        private static StatementSyntax[] CreateMessage(ContextCore ctx)
        {
            var result = new List<ExpressionStatementSyntax>();
            if (ctx.HavePrimaryConstructor)
            {
                List<ArgumentSyntax> args = new();
                var constructorParams = ctx.ConstructorParams;
                var assignments = new List<ExpressionStatementSyntax>();
                foreach (var member in ctx.Members)
                {
                    var parameter = constructorParams!.Value.FirstOrDefault(param => param.Name.Equals(member.NameSym.Name));
                    if (parameter != default)
                    {
                        args.Add(Argument(member.AssignedVariableToken, NameColon(parameter)));
                    }
                    else
                    {
                        assignments.Add(
                            SimpleAssignExprStatement(
                                SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                                IdentifierName(member.AssignedVariableToken)));
                    }
                }

                var creation = SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration, args.ToArray()));
                result.Add(creation);
                result.AddRange(assignments);
            }
            else
            {
                result.Add(SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration)));
                foreach (var member in ctx.Members)
                {
                    result.Add(
                        SimpleAssignExprStatement(
                            SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                            IdentifierName(member.AssignedVariableToken)));
                }
            }
            return result.ToArray();
        }
        private static StatementSyntax[] Operations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            foreach (var member in ctx.Members)
            {
                if (TryGenerateParseEnum(member.ByteName.Length, member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                {
                    continue;
                }
                if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                {
                    continue;
                }
                if (TryGenerateTryParseBson(member, bsonName, builder))
                {
                    continue;
                }
                GeneratorDiagnostics.ReportUnsuporterTypeError(member.NameSym, member.TypeSym);
            }
            return builder.ToArray();
        }

        private static bool TryGenerateParseEnum(int byteNameLength, SyntaxToken staticNameSpan, SyntaxToken readTarget, SyntaxToken bsonName, ISymbol nameSym, ITypeSymbol typeSym, ImmutableList<StatementSyntax>.Builder builder)
        {
            if (TryGetEnumReadOperation(readTarget, nameSym, typeSym, false, out var enumOp) == false)
            {
                return false;
            }
            StatementSyntax ifOperation = enumOp.TempExpr != null ? IfNotReturnFalseElse(enumOp.Expr, Block(SimpleAssignExpr(readTarget, enumOp.TempExpr))) : IfNotReturnFalse(enumOp.Expr);
            builder.IfStatement(condition: SpanSequenceEqual(bsonName, staticNameSpan, byteNameLength),
                                statement: Block(ifOperation, ContinueStatement));
            return true;
        }

        private static bool TryGenerateSimpleReadOperation(ContextCore ctx, MemberContext member, SyntaxToken bsonType, SyntaxToken bsonName, ImmutableList<StatementSyntax>.Builder builder)
        {
            var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
            var (operation, tempVar) = ReadOperation(ctx, member.NameSym, trueType, BsonReaderToken, IdentifierName(member.AssignedVariableToken), bsonType);
            if (operation != default)
            {
                builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken, member.ByteName.Length),
                                    statement: tempVar != null
                                        ? Block(IfNotReturnFalse(operation), SimpleAssignExprStatement(member.AssignedVariableToken, tempVar), ContinueStatement)
                                        : Block(IfNotReturnFalse(operation), ContinueStatement));
                return true;
            }
            return false;
        }
        private static bool TryGenerateTryParseBson(MemberContext member, SyntaxToken bsonName, ImmutableList<StatementSyntax>.Builder builder)
        {
            var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
            ITypeSymbol type = default;
            if (IsBsonSerializable(trueType))
            {
                type = trueType;
            }

            if (IsBsonExtensionSerializable(member.NameSym, trueType, out var extSym))
            {
                type = extSym;
            }

            if (type is null)
            {
                return false;
            }

            if (trueType.IsReferenceType)
            {
                var condition = InvocationExpr(IdentifierName(type.ToString()), IdentifierName("TryParseBson"),
                                               RefArgument(BsonReaderToken),
                                               OutArgument(member.AssignedVariableToken));
                builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken, member.ByteName.Length),
                                    statement: Block(IfNotReturnFalse(condition), ContinueStatement));
                return true;
            }
            else
            {
                var localTryParseVar = Identifier($"{member.AssignedVariableToken.ToString()}TryParseTemp");
                var condition = InvocationExpr(IdentifierName(type.ToString()), IdentifierName("TryParseBson"),
                                               RefArgument(BsonReaderToken), OutArgument(VarVariableDeclarationExpr(localTryParseVar)));

                builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken, member.ByteName.Length),
                                    statement:
                                        Block(
                                            IfNotReturnFalse(condition),
                                            SimpleAssignExprStatement(member.AssignedVariableToken, localTryParseVar),
                                            ContinueStatement));
                return true;
            }
        }
        private static ReadOperationContext ReadOperation(ContextCore ctx, ISymbol nameSym, ITypeSymbol trueTypeSym, SyntaxToken readerId, ExpressionSyntax readTarget, SyntaxToken bsonType)
        {
            /*
             * DO NOT REORDER CONDITIONS
             * **/
            if (ctx.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueTypeSym.Name)) != default) // generic type arguments
            {
                var temp = Identifier($"{nameSym.Name}TempGenericNullable");
                if (trueTypeSym.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return new(TryReadGenericNullable(TypeName(trueTypeSym.OriginalDefinition), bsonType, VarVariableDeclarationExpr(temp)), IdentifierName(temp));
                }
                else
                {
                    return TryReadGeneric(bsonType, readTarget);
                }
            }
            if (IsListCollection(trueTypeSym))
            {
                return InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, trueTypeSym)), RefArgument(readerId), OutArgument(readTarget));
            }
            if (TryGetSimpleReadOperation(nameSym, trueTypeSym, IdentifierName(bsonType), readTarget, out var simpleOperation))
            {
                return simpleOperation;
            }
            return default;
        }
        private static bool TryGetSimpleReadOperation(ISymbol nameSym, ITypeSymbol typeSymbol, ExpressionSyntax bsonType, ExpressionSyntax variable, out ExpressionSyntax expr)
        {
            expr = default;
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Double:
                    expr = TryGetDouble(variable);
                    return true;
                case SpecialType.System_String:
                    expr = TryGetString(variable);
                    return true;
                case SpecialType.System_Boolean:
                    expr = TryGetBoolean(variable);
                    return true;
                case SpecialType.System_Int32:
                    expr = TryGetInt32(variable);
                    return true;
                case SpecialType.System_Int64:
                    expr = TryGetInt64(variable);
                    return true;
                    //case SpecialType.System_DateTime:
                    //    expr = TryGetDateTimeWithBsonType(bsonType, variable);
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
                        GeneratorDiagnostics.ReportUnsuportedByteArrayReprError(nameSym, typeSymbol);
                        break;
                }
                expr = TryGetBinaryData(arrayRepr, variable);
                return true;
            }
            if (IsBsonTimestamp(typeSymbol))
            {
                expr = TryGetTimestamp(variable);
                return true;
            }
            if (IsBsonDocument(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (IsBsonArray(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (IsGuid(typeSymbol))
            {
                expr = TryGetGuidWithBsonType(bsonType, variable);
                return true;
            }
            if (IsDateTimeOffset(typeSymbol))
            {
                expr = TryGetDateTimeWithBsonType(bsonType, variable);
                return true;
            }
            if (IsBsonObjectId(typeSymbol))
            {
                expr = TryGetObjectId(variable);
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
