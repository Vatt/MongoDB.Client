using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static string SerializerName(ISymbol sym)
        {
            return $"{sym.Name}SerializerGenerated";
        }
        private static TypeArgumentListSyntax GetInterfaceParameters(GenerationContext ctx)
        {
            var decl = ctx.Declaration;
            if (ctx.GenericArgs is null)
            {
                return SF.TypeArgumentList(SF.SingletonSeparatedList(TypeFullName(decl)));
            }

            var args = ctx.GenericArgs.Select(arg => TypeName(arg));
            return SF.TypeArgumentList().AddArguments(GenericName(TokenName(decl), args.ToArray() ));
        }
        private static  TypeParameterListSyntax GetTypeParametersList(GenerationContext ctx)
        {
            var decl = ctx.Declaration;
            var paramsList = new SeparatedSyntaxList<TypeParameterSyntax>();
            foreach (var param in decl.TypeParameters)
            {
                paramsList = paramsList.Add(SF.TypeParameter(param.Name));
            }
            return SF.TypeParameterList(paramsList);
        }
        public static ClassDeclarationSyntax GenerateClass(GenerationContext ctx)
        {
            return SF.ClassDeclaration(SerializerName(ctx.Declaration))
                     .WithTypeParameterList(GetTypeParametersList(ctx));
        }
    }
}