using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
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
        public static List<MethodDeclarationSyntax> CollectionMethods(MemberContext member, ITypeSymbol type)
        {
            var mode = member.Root.GeneratorMode;
            var methods = new List<MethodDeclarationSyntax>();

            if (IsListCollection(type))
            {
                if (mode.GenerateTryParseBson) methods.Add(TryParseListCollectionMethod(member, type));
                if (mode.GenerateWriteBson) methods.Add(WriteListCollectionMethod(member, type));

                return methods;
            }
            if (IsDictionaryCollection(type))
            {
                if (mode.GenerateTryParseBson) methods.Add(TryParseDictionaryMethod(member, type));
                if (mode.GenerateWriteBson) methods.Add(WriteDictionaryMethod(member, type));

                return methods;
            }

            ReportUnsupportedTypeError(member.NameSym, member.TypeSym);

            return default;
        }

        public static MethodDeclarationSyntax[] GenerateCollectionMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            HashSet<ITypeSymbol> declared = new(SymbolEqualityComparer.Default);

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
        private static bool TryGenerateCollectionTryParseBson(ITypeSymbol typeSym,
                                                              ISymbol nameSym,
                                                              SyntaxToken collectionToken,
                                                              SyntaxToken readTarget,
                                                              List<StatementSyntax> statements,
                                                              params ArgumentSyntax[] collectionAddArgs)
        {
            ITypeSymbol callType = default;
            ITypeSymbol outArgType = default;
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

            var operation = InvocationExpr(IdentifierName(callType.ToString()), TryParseBsonToken,
                                           RefArgument(BsonReaderToken),
                                           OutArgument(TypedVariableDeclarationExpr(TypeFullName(outArgType),
                                                       readTarget)));

            statements.Add(IfNotReturnFalseElse(condition: operation, @else: Block(InvocationExprStatement(collectionToken, CollectionAddToken, collectionAddArgs), ContinueStatement)));

            return true;
        }
        private static bool TryGenerateCollectionSimpleRead(MemberContext ctx,
                                                            ITypeSymbol type,
                                                            SyntaxToken CollectionToken,
                                                            SyntaxToken readTarget,
                                                            SyntaxToken bsonTypeToken,
                                                            List<StatementSyntax> statements,
                                                            params ArgumentSyntax[] collectionAddArgs)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, BsonReaderToken, TypedVariableDeclarationExpr(TypeFullName(type), readTarget), bsonTypeToken);

            Debug.Assert(tempVar is null);

            if (operation == default)
            {
                return false;
            }

            statements.Add(IfNotReturnFalseElse(condition: operation,
                                                @else:
                                                Block(InvocationExprStatement(CollectionToken, CollectionAddToken, collectionAddArgs),
                                                      ContinueStatement)));
            return true;
        }
    }

}
