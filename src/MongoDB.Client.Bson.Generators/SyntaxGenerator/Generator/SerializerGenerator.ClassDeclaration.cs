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
        public static string SerializerName(ClassContext ctx)
        {
            string generics = ctx.GenericArgs.HasValue && ctx.GenericArgs.Value.Length > 0
                ? string.Join(String.Empty, ctx.GenericArgs.Value)
                : String.Empty;
            return $"{ctx.Declaration.ContainingNamespace.ToString().Replace('.', '_')}{ctx.Declaration.Name}{generics}SerializerGenerated";
        }

        public static SyntaxToken StaticFieldNameToken(MemberContext ctx)
        {
            return SF.Identifier($"{ctx.Root.Declaration.Name}{ctx.BsonElementAlias}");
        }
        private static BaseListSyntax BaseList(ClassContext ctx)
        {
            var decl = ctx.Declaration;
            var serializer = SF.Identifier("MongoDB.Client.Bson.Serialization.IGenericBsonSerializer");
            if (ctx.GenericArgs.HasValue && !ctx.GenericArgs.Value.IsEmpty)
            {
                return   SF.BaseList().AddTypes(SF.SimpleBaseType(GenericName(serializer, TypeFullName(decl))));
            }
            var name = GenericName(serializer,TypeFullName(decl));
            return SF.BaseList().AddTypes(SF.SimpleBaseType(GenericName(serializer, name)));
        }
        public static ClassDeclarationSyntax GenerateSerializer(ClassContext ctx)
        {
            var decl =  SF.ClassDeclaration(SerializerName(ctx))
                .AddModifiers(PublicKeyword(), SealedKeyword())
                .WithBaseList(BaseList(ctx))
                .WithMembers(GenerateStaticNamesSpans());
            return ctx.GenericArgs!.Value.Length > 0
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