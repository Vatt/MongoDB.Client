using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal static  class AttributeHelper
    {
        public static string BsonSerializableAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute";
        public static string BsonEnumSerializableAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonEnumSerializableAttribute";
        public static string BsonConstructorAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonConstructorAttribute";
        public static string IgnoreAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonIgnoreAttribute";
        public static string BsonElementAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonElementAttribute";
        public static string BsonIdAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonIdAttribute";
        public static string BsonWriteIgnoreIfAttr = "MongoDB.Client.Bson.Serialization.Attributes.BsonWriteIgnoreIfAttribute";
        public static bool TryGetBsonWriteIgnoreIfAttr(MemberContext ctx, out ExpressionSyntax expr)
        {
            expr = default;
            if (ctx.NameSym.GetAttributes().Length == 0)
            {
                return false;
            }
            foreach(var attr in ctx.NameSym.GetAttributes())
            {
                if (attr.AttributeClass.ToString().Equals(BsonWriteIgnoreIfAttr))
                {
                    expr = SF.ParseExpression((string)attr.ConstructorArguments[0].Value);
                    
                    foreach (var meta in ctx.Root.Root.Contexts.Where(classСtx => classСtx.Declaration.ToString().Equals(ctx.NameSym.ToString())))
                    {
                        foreach (var member in ctx.Root.Root.Contexts)
                        {
                            var id = SF.IdentifierName(member.Declaration.Name);
                            var newid = SF.IdentifierName($"{ctx.Root.WriterInputVar.Identifier.Text}.{member.Declaration.Name}");
                            foreach (var node in expr.DescendantNodes())
                            {
                                if (node.ToString().Equals(member.Declaration.Name))
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
                if (attr.AttributeClass!.ToString().Equals(BsonEnumSerializableAttr))
                {
                    return (int) attr.ConstructorArguments[0].Value;
                }
            }

            return -1;
        }
        
        public static bool TryFindPrimaryConstructor(INamedTypeSymbol symbol, out IMethodSymbol? constructor)
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
                    if (attr.AttributeClass.ToString().Equals(BsonConstructorAttr))
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
                if (attr.AttributeClass!.ToString().Equals(BsonIdAttr))
                {
                    return ("_id", "_id");
                }
            }
            foreach (var attr in memberSym.GetAttributes())
            {
                if (attr.AttributeClass!.ToString().Equals(BsonElementAttr))
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
                if (attr.AttributeClass!.ToString().Equals(IgnoreAttr))
                {
                    return true;
                }
            }
            return false;
        }
    }
}