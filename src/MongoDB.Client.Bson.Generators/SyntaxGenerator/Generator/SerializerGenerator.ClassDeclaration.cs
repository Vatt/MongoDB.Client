using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly SyntaxToken SerializerToken = SF.Identifier("MongoDB.Client.Bson.Serialization.IGenericBsonSerializer");
        public static string SerializerName(ClassContext ctx)
        {
            string generics = ctx.GenericArgs.HasValue && ctx.GenericArgs.Value.Length > 0
                ? string.Join(String.Empty, ctx.GenericArgs.Value)
                : String.Empty;
            return $"{ctx.Declaration.ContainingNamespace.ToString().Replace(".", String.Empty)}{ctx.Declaration.Name}{generics}SerializerGenerated";
        }

        public static InvocationExpressionSyntax GeneratedSerializerTryParse(ClassContext ctx, ExpressionSyntax reader, ExpressionSyntax variable)
        {
            var serializer = SF.IdentifierName($"{SerializerName(ctx)}StaticField");
            return InvocationExpr(serializer, IdentifierName("TryParse"), RefArgument(reader), OutArgument(variable));
        }
        public static SyntaxToken StaticFieldNameToken(MemberContext ctx)
        {
            return SF.Identifier($"{ctx.Root.Declaration.Name}{ctx.BsonElementAlias}");
        }
        private static BaseListSyntax BaseList(ClassContext ctx)
        {
            var decl = ctx.Declaration;
            if (ctx.GenericArgs.HasValue && ctx.GenericArgs.Value.Length > 0)
            {
                return   SF.BaseList().AddTypes(SF.SimpleBaseType(GenericName(SerializerToken, TypeFullName(decl))));
            }
            var name = GenericName(SerializerToken,TypeFullName(decl));
            return SF.BaseList().AddTypes(SF.SimpleBaseType(GenericName(SerializerToken, name)));
        }
        public static ClassDeclarationSyntax GenerateSerializer(ClassContext ctx)
        {
            var decl = SF.ClassDeclaration(SerializerName(ctx))
                .AddModifiers(PublicKeyword(), SealedKeyword())
                .WithBaseList(BaseList(ctx))
                .WithMembers(GenerateStaticNamesSpans())
                .AddMembers(TryParseMethod(ctx))
                .AddMembers(GenerateReadArrayMethods(ctx));
            return ctx.GenericArgs.HasValue && ctx.GenericArgs!.Value.Length > 0
                ? decl.AddTypeParameterListParameters(ctx.GenericArgs!.Value.Select(TypeParameter).ToArray())
                : decl;
            
            SyntaxList<MemberDeclarationSyntax> GenerateStaticNamesSpans()
            {
                var list = new SyntaxList<MemberDeclarationSyntax>();
                foreach (var member in ctx.Members)
                {
                    var bytes = Encoding.UTF8.GetBytes(member.BsonElementValue);
                    list = list.Add(
                        SF.PropertyDeclaration(
                            attributeLists: default,
                            modifiers: new(PrivateKeyword(), StaticKeyword()),
                            type: ReadOnlySpanByte(),
                            explicitInterfaceSpecifier: default,
                            identifier: StaticFieldNameToken(member),
                            accessorList: default,
                            expressionBody: SF.ArrowExpressionClause(SingleDimensionByteArrayCreation(bytes.Length, SeparatedList(bytes.Select(NumericLiteralExpr)))),
                            initializer: default,
                            semicolonToken: SemicolonToken()));
                }
                return list;
            }
        }
    }
}