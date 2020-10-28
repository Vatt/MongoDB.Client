using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    partial class BsonSyntaxGenerator
    {
        private static CompilationUnitSyntax GenerateRootUnit(ClassDeclMeta classmeta)
        {

            return SF.CompilationUnit()
                .AddUsings(
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Reader")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Serialization")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Document")),
                    SF.UsingDirective(SF.ParseName("System")),
                    SF.UsingDirective(SF.ParseName("System.Collections.Generic")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client")),
                    SF.UsingDirective(SF.ParseName(classmeta.StringNamespace)));
            //                .AddMembers(SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated")));
        }
        private static NamespaceDeclarationSyntax GenerateNamespace(ClassDeclMeta classmeta)
        {
            return SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated"));
        }
        public static CompilationUnitSyntax Create(ClassDeclMeta classmeta)
        {
            if (classmeta.MemberDeclarations.Count == 0)
            {
                return default;
            }
            return GenerateRootUnit(classmeta)
                    .AddMembers(
                        GenerateNamespace(classmeta)
                            .AddMembers(ClassDeclarationGenerator.Create(classmeta))
                    );
        }
    }
}
