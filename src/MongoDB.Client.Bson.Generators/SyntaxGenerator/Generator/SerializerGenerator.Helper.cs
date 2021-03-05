using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        //private const string BsonSerializableAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute";
        //private const string BsonEnumAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonEnumAttribute";
        //private const string BsonConstructorAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonConstructorAttribute";
        //private const string IgnoreAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonIgnoreAttribute";
        //private const string BsonElementAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonElementAttribute";
        //private const string BsonIdAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonIdAttribute";
        //private const string BsonWriteIgnoreIfAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonWriteIgnoreIfAttribute";
        public static INamedTypeSymbol BsonSerializableAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute")!;
        public static INamedTypeSymbol BsonSerializerExtAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializerExtAttribute")!;
        public static INamedTypeSymbol BsonEnumAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonEnumAttribute")!;
        public static INamedTypeSymbol BsonConstructorAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonConstructorAttribute")!;
        public static INamedTypeSymbol IgnoreAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonIgnoreAttribute")!;
        public static INamedTypeSymbol BsonElementAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonElementAttribute")!;
        public static INamedTypeSymbol BsonIdAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonIdAttribute")!;
        public static INamedTypeSymbol BsonWriteIgnoreIfAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonWriteIgnoreIfAttribute")!;

        public static bool IsBsonExtensionSerializable(ISymbol nameSym, ISymbol typeSym, out ITypeSymbol extType)
        {
            extType = default;
            var bsonExtAttr = BsonSerializerExtAttr;
            foreach (var attr in nameSym.GetAttributes())
            {
                if (attr.AttributeClass is not null && attr.AttributeClass.Equals(bsonExtAttr, SymbolEqualityComparer.Default))
                {
                    extType = BsonSerializerGenerator.Compilation.GetTypeByMetadataName(attr.ConstructorArguments[0].Value?.ToString());
                    if (extType == null)
                    {
                        return false;
                    }

                    if (HaveParseWriteMethods(extType, typeSym) == false)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool IsBsonSerializable(ISymbol typeSym)
        {
            var bsonAttr = BsonSerializableAttr;
            foreach (var attr in typeSym.GetAttributes())
            {
                if (attr.AttributeClass is not null && attr.AttributeClass.Equals(bsonAttr, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }

            if (HaveParseWriteMethods(typeSym))
            {
                return true;
            }
            return false;
        }

        public static bool HaveParseWriteMethods(ISymbol typeSym, ISymbol? retType = null)
        {
            ISymbol returnType = retType ?? typeSym;
            if (typeSym is INamedTypeSymbol namedSym)
            {
                var readerSym = BsonReaderTypeSym;
                var writerSym = BsonWriterTypeSym;
                var parseMethod = namedSym.GetMembers()
                    .Where(member => member.Kind == SymbolKind.Method)
                    .Where(method => method.Name.Equals("TryParseBson", System.StringComparison.InvariantCulture) && method.Kind == SymbolKind.Method && method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
                    .Where(method =>
                    {

                        var parseMethodSym = method as IMethodSymbol;
                        if (parseMethodSym != null && parseMethodSym.ReturnType.SpecialType != SpecialType.System_Boolean)
                        {
                            return false;
                        }
                        if (parseMethodSym!.Parameters.Length != 2)
                        {
                            return false;
                        }
                        bool haveReader = false;
                        bool haveResult = false;
                        if (parseMethodSym.Parameters[0].RefKind == RefKind.Ref && parseMethodSym.Parameters[0].Type.Equals(readerSym, SymbolEqualityComparer.Default))
                        {
                            haveReader = true;
                        }
                        if (parseMethodSym.Parameters[1].RefKind == RefKind.Out && parseMethodSym.Parameters[1].Type.ToString().Equals(returnType.ToString()))
                        {
                            haveResult = true;
                        }
                        if (haveReader && haveResult)
                        {
                            return true;
                        }
                        return false;
                    })
                    .FirstOrDefault();
                var writeMethod = namedSym.GetMembers()
                    .Where(member => member.Kind == SymbolKind.Method)
                    .Where(method => method.Name.Equals("WriteBson", System.StringComparison.InvariantCulture) && method.Kind == SymbolKind.Method && method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
                    .Where(method =>
                    {
                        var writeMethodSym = method as IMethodSymbol;
                        if (writeMethodSym != null && writeMethodSym.ReturnType.SpecialType != SpecialType.System_Void)
                        {
                            return false;
                        }
                        if (writeMethodSym!.Parameters.Length != 2)
                        {
                            return false;
                        }
                        bool haveReader = false;
                        bool haveResult = false;
                        if (writeMethodSym.Parameters[0].RefKind == RefKind.Ref && writeMethodSym.Parameters[0].Type.Equals(writerSym, SymbolEqualityComparer.Default))
                        {
                            haveReader = true;
                        }
                        if (writeMethodSym.Parameters[1].RefKind == RefKind.In && writeMethodSym.Parameters[1].Type.ToString().Equals(returnType.ToString()))
                        {
                            haveResult = true;
                        }
                        if (haveReader && haveResult)
                        {
                            return true;
                        }
                        return false;
                    })
                    .FirstOrDefault();
                return parseMethod != null && writeMethod != null;
            }

            return false;
        }
        public static bool TryGetBsonWriteIgnoreIfAttr(MemberContext ctx, out ExpressionSyntax expr)
        {
            var bsonWriteIgnoreIfAttr = BsonWriteIgnoreIfAttr;
            expr = default;
            if (ctx.NameSym.GetAttributes().Length == 0)
            {
                return false;
            }
            foreach (var attr in ctx.NameSym.GetAttributes())
            {
                if (attr.AttributeClass != null && attr.AttributeClass.Equals(bsonWriteIgnoreIfAttr, SymbolEqualityComparer.Default))
                {
                    expr = SF.ParseExpression((string)attr.ConstructorArguments[0].Value);

                    foreach (var member in ctx.Root.Members)
                    {
                        var newid = SF.IdentifierName($"{WriterInputVarToken.Text}.{member.NameSym.Name}");
                        foreach (var node in expr.DescendantNodes())
                        {
                            if (node.ToString().Equals(member.NameSym.Name))
                            {
                                if (node is ArgumentSyntax arg)
                                {
                                    var newarg = SF.Argument(newid);
                                    expr = expr.ReplaceNode(node, newarg);
                                }
                                else
                                {
                                    expr = expr.ReplaceNode(node, newid);
                                }
                            }
                        }
                    }
                    expr = SF.ParenthesizedExpression(expr);
                    return true;
                }
            }
            return false;
        }
        public static int GetEnumRepresentation(ISymbol symbol)
        {
            var bsonAttr = BsonEnumAttr;
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(bsonAttr, SymbolEqualityComparer.Default))
                {
                    return (int)attr.ConstructorArguments[0].Value;
                }
            }

            return -1;
        }

        public static bool TryFindPrimaryConstructor(INamedTypeSymbol symbol, SyntaxNode node, out IMethodSymbol constructor)
        {
            constructor = default;
            if (symbol.Constructors.Length == 0)
            {
                return false;
            }


            if (node is RecordDeclarationSyntax recordDecl)
            {
                if (recordDecl.ParameterList != null)
                {
                    constructor = symbol.Constructors[0];
                    return true;
                }
            }

            if (symbol.Constructors.Length == 1 && symbol.TypeKind == TypeKind.Class && node is not RecordDeclarationSyntax)
            {
                constructor = symbol.Constructors[0];
                return true;
            }
            var constructorAttr = BsonConstructorAttr;
            foreach (var item in symbol.Constructors)
            {
                foreach (var attr in item.GetAttributes())
                {
                    if (attr.AttributeClass == null)
                    {
                        continue;
                    }
                    //TODO: проверить на множественное вхождение атрибутов
                    if (attr.AttributeClass.Equals(constructorAttr, SymbolEqualityComparer.Default))
                    {
                        constructor = item;
                        return true;
                    }
                }
            }
            //if (node is RecordDeclarationSyntax recordDecl)
            //{
            //    if (symbol.Constructors.Length == 2)
            //    {
            //        constructor = symbol.Constructors.First(x => !SymbolEqualityComparer.Default.Equals(x.Parameters[0].Type, symbol));
            //        return true;
            //    }
            //    if (recordDecl.ParameterList != null)
            //    {
            //        var parameters = recordDecl.ParameterList.Parameters;
            //        foreach (var ctor in symbol.Constructors)
            //        {
            //            bool found = true;
            //            if (ctor.Parameters.Length != parameters.Count)
            //            {
            //                continue;
            //            }
            //            foreach (var param in ctor.Parameters)
            //            {
            //                if (parameters.Where(p => p.Identifier.Text.Equals(param.Name) && p.Type.GetText().ToString().Trim().Equals(param.Type.ToString())) == default)
            //                {
            //                    found = false;
            //                    break;
            //                }

            //            }
            //            if (found)
            //            {
            //                constructor = ctor;
            //                return true;
            //            }
            //        }
            //    }
            //}
            return false;
        }

        public static (string, string) GetMemberAlias(ISymbol memberSym)
        {
            var bsonIdAttr = BsonIdAttr;
            var bsonElementAttr = BsonElementAttr;
            foreach (var attr in memberSym.GetAttributes())
            {
                //TODO: проверить на множественное вхождение атрибутов
                if (attr.AttributeClass!.Equals(bsonIdAttr, SymbolEqualityComparer.Default))
                {
                    return ("_id", "_id");
                }
            }
            foreach (var attr in memberSym.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(bsonElementAttr, SymbolEqualityComparer.Default))
                {
                    if (attr.ConstructorArguments.IsEmpty)
                    {
                        return (memberSym.Name, memberSym.Name);
                    }
                    var data = (string)attr.ConstructorArguments[0].Value!;
                    data = EscapeString(data);
                    return ((string)attr.ConstructorArguments[0].Value!, data!);
                }
            }
            return (memberSym.Name, memberSym.Name);
        }


        public static bool IsIgnore(ISymbol symbol)
        {
            var ignoreAttr = IgnoreAttr;
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(ignoreAttr, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
            return false;
        }

        private static string EscapeString(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (var item in value)
            {
                if (item == '(' || item == ')' || item == '$' || item == ' ')
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }
    }
}