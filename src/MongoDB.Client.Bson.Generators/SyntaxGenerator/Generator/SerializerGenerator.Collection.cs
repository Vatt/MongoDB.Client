using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public readonly struct CollectionReadContext
        {
            public readonly SyntaxToken DocLenToken;
            public readonly SyntaxToken UnreadedToken;
            public readonly SyntaxToken EndMarkerToken;
            public readonly SyntaxToken BsonTypeToken;
            public readonly SyntaxToken BsonNameToken;
            public readonly SyntaxToken OutMessageToken;
            public readonly SyntaxToken TempCollectionReadTargetToken;
            public readonly SyntaxToken TempCollectionToken;
            public readonly ArgumentSyntax[] CollectionAddArguments;

            public CollectionReadContext(
                SyntaxToken docLenToken, SyntaxToken unreadedToken, SyntaxToken endMarkerToken,
                SyntaxToken bsonTypeToken, SyntaxToken bsonNameToken, SyntaxToken outMessageTokenToken,
                SyntaxToken tempCollectionReadTargetToken, SyntaxToken tempCollectionToken,
                ArgumentSyntax[] collectionAddArguments)
            {
                DocLenToken = docLenToken;
                UnreadedToken = unreadedToken;
                EndMarkerToken = endMarkerToken;
                BsonTypeToken = bsonTypeToken;
                BsonNameToken = bsonNameToken;
                OutMessageToken = outMessageTokenToken;
                TempCollectionReadTargetToken = tempCollectionReadTargetToken;
                TempCollectionToken = tempCollectionToken;
                CollectionAddArguments = collectionAddArguments;
            }
        }
        public static string UnwrapTypeName(ITypeSymbol type)
        {
            string name = type.Name;
            if (type is INamedTypeSymbol namedType)
            {
                foreach (var arg in namedType.TypeArguments)
                {
                    name += arg switch
                    {
                        INamedTypeSymbol argNamedType => UnwrapTypeName(argNamedType),
                        IArrayTypeSymbol argArrayType => $"Array{UnwrapTypeName(argArrayType.ElementType)}",
                        _ => arg.Name,
                    };
                }
            }

            return name;
        }
        private static NameSyntax ConstructCollectionType(ISymbol sym)
        {
            Debug.Assert(IsCollection(sym));
            var typed = sym as INamedTypeSymbol;
            if (IsDictionaryCollection(sym))
            {
                ExtractDictionaryTypeArgs(typed, out var keyArg, out var typeArg);
                var dictSym = System_Collections_Generic_Dictionary_K_V.Construct(keyArg, typeArg);
                return IdentifierName(dictSym.ToString());
            }

            if (IsListCollection(sym))
            {
                var listSym = System_Collections_Generic_List_T.Construct(typed!.TypeArguments[0]);
                return IdentifierName(listSym.ToString());
            }

            return default;
        }
        public static SyntaxToken CollectionTryParseMethodName(ITypeSymbol typeSymbol)
        {
            Debug.Assert(IsCollection(typeSymbol));
            Debug.Assert(typeSymbol is INamedTypeSymbol);
            var type = typeSymbol as INamedTypeSymbol;
            var name = $"TryParse{UnwrapTypeName(type)}";
            return Identifier(name);
        }
        public static SyntaxToken CollectionWriteMethodName(ITypeSymbol typeSymbol)
        {
            Debug.Assert(IsCollection(typeSymbol));
            Debug.Assert(typeSymbol is INamedTypeSymbol);
            var type = typeSymbol as INamedTypeSymbol;
            var name = $"Write{UnwrapTypeName(type)}";
            return Identifier(name);
        }
        public static MethodDeclarationSyntax[] CollectionMethods(MemberContext member, ITypeSymbol type)
        {
            if (IsListCollection(type))
            {
                return new[]
                {
                    TryParseListCollectionMethod(member, type),
                    WriteListCollectionMethod(member, type)
                };
            }
            if (IsDictionaryCollection(type))
            {
                return new[]
                {
                    TryParseDictionaryMethod(member, type),
                    WriteDictionaryMethod(member, type)
                };
            }

            ReportUnsuporterTypeError(member.NameSym, member.TypeSym);
            return default;
        }

        public static MethodDeclarationSyntax[] GenerateCollectionMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            HashSet<ITypeSymbol> declared = new();
            foreach (var member in ctx.Members.Where(x => IsCollection(x.TypeSym)))
            {
                var type = member.TypeSym as INamedTypeSymbol;
                if (type is null && declared.Contains(type, SymbolEqualityComparer.Default) is false)
                {
                    methods.AddRange(CollectionMethods(member, member.TypeSym));
                    declared.Add(type);
                    continue;
                }
                while (true)
                {
                    if (declared.Contains(type, SymbolEqualityComparer.Default) is false)
                    {
                        methods.AddRange(CollectionMethods(member, type));
                        declared.Add(type);
                    }
                    if (IsListCollection(type))
                    {
                        type = type!.TypeArguments[0] as INamedTypeSymbol;
                    }
                    else if (IsDictionaryCollection(type))
                    {
                        ExtractDictionaryTypeArgs(type, out _, out var tempType);
                        type = tempType as INamedTypeSymbol;
                    }
                    else
                    {
                        type = null;
                    }

                    if (type is null || (IsCollection(type) == false))
                    {
                        break;
                    }
                }
            }
            return methods.ToArray();
        }
        private static bool TryGenerateCollectionTryParseBson(MemberContext ctx, ITypeSymbol typeSym, ISymbol nameSym, CollectionReadContext readCtx, ImmutableList<StatementSyntax>.Builder builder, bool continueGeneration = false, bool inSwitch = false)
        {
            ITypeSymbol callType = default;
            ITypeSymbol outArgType = default;
            var methodToken = continueGeneration ? TryContinueParseBsonToken : TryParseBsonToken;
            if (IsBsonSerializable(typeSym))
            {
                callType = typeSym;
                outArgType = typeSym;
            }

            if (IsBsonExtensionSerializable(nameSym, typeSym, out var extSym))
            {
                callType = extSym;
                outArgType = typeSym;
            }

            if (callType is null || outArgType is null)
            {
                return false;
            }

            var operation = InvocationExpr(IdentifierName(callType.ToString()), methodToken,
                                           RefArgument(BsonReaderToken),
                                           continueGeneration ? Argument(SimpleMemberAccess(StateToken, CollectionStateArgumentToken)) : OutArgument(SimpleMemberAccess(StateToken, CollectionStateArgumentToken)),
                                           OutArgument(PositionToken));
            if (inSwitch)
            {
                builder.IfNotReturnFalseElse(operation, Block(
                                           InvocationExprStatement(CollectionLowStateMemberAccess(ctx), CollectionAddToken, Argument(CreateMessageFromState(outArgType, CollectionStateArgumentAccess))),
                                           SimpleAssignExprStatement(CollectionStateArgumentAccess, NullLiteralExpr()),
                                           AddAssignmentExprStatement(StateConsumedMemberAccess, BinaryExprMinus(ReaderBytesConsumedExpr, StartCheckpointToken)),
                                           BreakStatement
                                           ));
            }
            else
            {
                builder.IfNotElse(
                          condition: operation,
                          statements: Block(AddAssignmentExprStatement(StateConsumedMemberAccess, BinaryExprMinus(ReaderBytesConsumedExpr, StartCheckpointToken)), ReturnFalseStatement),
                          @else: continueGeneration ? Block(InvocationExprStatement(CollectionLowStateMemberAccess(ctx), CollectionAddToken, Argument(CreateMessageFromState(typeSym, CollectionStateArgumentAccess))),
                                                            AddAssignmentExprStatement(StateConsumedMemberAccess, BinaryExprMinus(ReaderBytesConsumedExpr, StartCheckpointToken)),
                                                            BreakStatement) :
                                                      Block(InvocationExprStatement(CollectionLowStateMemberAccess(ctx), CollectionAddToken, Argument(CreateMessageFromState(outArgType, CollectionStateArgumentAccess))),
                                                      SimpleAssignExprStatement(CollectionStateArgumentAccess, NullLiteralExpr()),
                                                      ContinueStatement));
            }
      
            return true;
        }
        private static bool TryGenerateCollectionSimpleRead(MemberContext ctx, ITypeSymbol type, CollectionReadContext readCtx, ImmutableList<StatementSyntax>.Builder builder)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, BsonReaderToken, TypedVariableDeclarationExpr(TypeFullName(type), readCtx.TempCollectionReadTargetToken), readCtx.BsonTypeToken);
            Debug.Assert(tempVar is null);
            if (operation == default)
            {
                return false;
            }

            builder.IfNotElse(
                condition: operation,
                statements: Block(SimpleAssignExprStatement(StateConsumedMemberAccess, BinaryExprPlus(SmallConsumedToken, LocalConsumedToken)), ReturnFalseStatement),
                @else:
                Block(
                    InvocationExprStatement(CollectionLowStateMemberAccess(ctx), CollectionAddToken, readCtx.CollectionAddArguments),
                    ContinueStatement));
            return true;
        }
    }

}
