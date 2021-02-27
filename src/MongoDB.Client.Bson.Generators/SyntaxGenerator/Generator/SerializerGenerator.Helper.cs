using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
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
        public static INamedTypeSymbol BsonEnumAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonEnumAttribute")!;
        public static INamedTypeSymbol BsonConstructorAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonConstructorAttribute")!;
        public static INamedTypeSymbol IgnoreAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonIgnoreAttribute")!;
        public static INamedTypeSymbol BsonElementAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonElementAttribute")!;
        public static INamedTypeSymbol BsonIdAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonIdAttribute")!;
        public static INamedTypeSymbol BsonWriteIgnoreIfAttr => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonWriteIgnoreIfAttribute")!;
        public static bool IsBsonSerializable(ISymbol symbol)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass is not null && attr.AttributeClass.Equals(BsonSerializableAttr, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
            if (symbol is INamedTypeSymbol namedSym)
            {
                var parseMethod = namedSym.GetMembers()
                    .Where(member => member.Kind == SymbolKind.Method)
                    .Where(method => method.Name.Equals("TryParseBson") && method.Kind == SymbolKind.Method && method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
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
                        if (parseMethodSym.Parameters[0].RefKind == RefKind.Ref && parseMethodSym.Parameters[0].Type.ToString().Equals("MongoDB.Client.Bson.Reader.BsonReader"))
                        {
                            haveReader = true;
                        }
                        if (parseMethodSym.Parameters[1].RefKind == RefKind.Out && parseMethodSym.Parameters[1].Type.ToString().Equals(symbol.ToString()))
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
                    .Where(method => method.Name.Equals("WriteBson") && method.Kind == SymbolKind.Method && method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
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
                        if (writeMethodSym.Parameters[0].RefKind == RefKind.Ref && writeMethodSym.Parameters[0].Type.ToString().Equals("MongoDB.Client.Bson.Writer.BsonWriter"))
                        {
                            haveReader = true;
                        }
                        if (writeMethodSym.Parameters[1].RefKind == RefKind.In && writeMethodSym.Parameters[1].Type.ToString().Equals(symbol.ToString()))
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
            expr = default;
            if (ctx.NameSym.GetAttributes().Length == 0)
            {
                return false;
            }
            foreach (var attr in ctx.NameSym.GetAttributes())
            {
                if (attr.AttributeClass != null && attr.AttributeClass.Equals(BsonWriteIgnoreIfAttr, SymbolEqualityComparer.Default))
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
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(BsonEnumAttr, SymbolEqualityComparer.Default))
                {
                    return (int)attr.ConstructorArguments[0].Value;
                }
            }

            return -1;
        }

        public static bool TryFindPrimaryConstructor(INamedTypeSymbol symbol, out IMethodSymbol constructor)
        {
            constructor = default;
            if (symbol.Constructors.Length == 0)
            {
                return false;
            }
            foreach (var item in symbol.Constructors)
            {
                foreach (var attr in item.GetAttributes())
                {
                    if (attr.AttributeClass == null)
                    {
                        continue;
                    }
                    //TODO: проверить на множественное вхождение атрибутов
                    if (attr.AttributeClass.Equals(BsonConstructorAttr, SymbolEqualityComparer.Default))
                    {
                        constructor = item;
                        return true;
                    }
                }
            }
            return false;
        }

        public static (string, string) GetMemberAlias(ISymbol memberSym)
        {
            foreach (var attr in memberSym.GetAttributes())
            {
                //TODO: проверить на множественное вхождение атрибутов
                if (attr.AttributeClass!.Equals(BsonIdAttr, SymbolEqualityComparer.Default))
                {
                    return ("_id", "_id");
                }
            }
            foreach (var attr in memberSym.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(BsonElementAttr, SymbolEqualityComparer.Default))
                {
                    if (attr.ConstructorArguments.IsEmpty)
                    {
                        return (memberSym.Name, memberSym.Name);
                    }
                    var data = (string)attr.ConstructorArguments[0].Value!;
                    data = data.Replace('(', '_').Replace(')', '_').Replace('$', '_').Replace(' ', '_');
                    return ((string)attr.ConstructorArguments[0].Value!, data!);
                }
            }
            return (memberSym.Name, memberSym.Name);
        }
        public static bool IsIgnore(ISymbol symbol)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass!.Equals(IgnoreAttr, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
            return false;
        }
    }
}