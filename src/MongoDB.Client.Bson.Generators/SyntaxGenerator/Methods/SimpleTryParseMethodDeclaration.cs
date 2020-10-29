using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class SimpleTryParseMethodDeclaration : TryParseMethodDeclatationBase
    {
        public SimpleTryParseMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl.ClassSymbol, classdecl.Members)
        {

        }
        public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier()
        {
            
            return SF.ExplicitInterfaceSpecifier(
                   SF.GenericName(
                       Basics.SerializerInterfaceIdentifier,
                       SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.ParseTypeName(ClassSymbol.Name)))),
                   SF.Token(SyntaxKind.DotToken));
        }
        public override TypeSyntax GetParseMethodOutParameter() => SF.ParseTypeName(ClassSymbol.Name);
    }
}
