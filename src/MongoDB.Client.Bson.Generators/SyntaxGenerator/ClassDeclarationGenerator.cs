using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ClassDeclarations;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal static class ClassDeclarationGenerator
    {
        public static ClassDeclarationSyntax Create(ClassDeclMeta classmeta)
        {
            ClassDeclarationBase classdecl;
            if (classmeta.ClassSymbol.IsGenericType)
            {
                classdecl = new GenericClassDeclaration(classmeta);
            }
            else
            {
                classdecl = new SimpleClassDeclaration(classmeta);
            }
            return classdecl.Build();
        }
    }
}
