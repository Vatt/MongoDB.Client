using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static string UnwrapTypeName(ITypeSymbol type)
        {
            string name = type.Name;
            if (type is INamedTypeSymbol namedType)
            {
                foreach (var arg in namedType.TypeArguments)
                {
                    switch (arg)
                    {
                        case INamedTypeSymbol argNamedType:
                            name += UnwrapTypeName(argNamedType);
                            break;
                        case IArrayTypeSymbol argArrayType:
                            name += $"Array{UnwrapTypeName(argArrayType.ElementType)}";
                            break;
                        default:
                            name += arg.Name;
                            break;
                    }
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
                var dictSym = System_Collections_Generic_Dictionary_K_V.Construct(System_String, typed!.TypeArguments[1]);
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

            GeneratorDiagnostics.ReportUnsuporterTypeError(member.NameSym, member.TypeSym);
            return default;
        }

        public static MethodDeclarationSyntax[] GenerateCollectionMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            HashSet<ITypeSymbol> declared = new();
            foreach(var member in ctx.Members.Where( x => IsCollection(x.TypeSym)))
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
                        type = type!.TypeArguments[1] as INamedTypeSymbol;
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
    }

}
