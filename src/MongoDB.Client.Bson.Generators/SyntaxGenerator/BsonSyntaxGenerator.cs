using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    partial class BsonSyntaxGenerator
    {
        private static CompilationUnitSyntax GenerateRootUnit()
        {

            return SF.CompilationUnit()
                .AddUsings(
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Reader")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Writer")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Serialization")),
                    SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Document")),
                    SF.UsingDirective(SF.ParseName("System")),
                    SF.UsingDirective(SF.ParseName("System.Collections.Generic")),
                    SF.UsingDirective(SF.ParseName("System.Buffers.Binary")));
                    //SF.UsingDirective(SF.ParseName(classmeta.StringNamespace)));
            //                .AddMembers(SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated")));
        }
        private static NamespaceDeclarationSyntax GenerateNamespace()
        {
            return SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated"));
        }
        public static CompilationUnitSyntax Create(ClassDeclMeta classmeta)
        {
            return GenerateRootUnit()
                    .AddMembers(
                        GenerateNamespace()
                            .AddMembers(ClassDeclarationGenerator.Create(classmeta))
                    );
        }

        public static CompilationUnitSyntax Create(MasterContext context)
        {
            return GenerateRootUnit()
                .AddMembers(GenerateNamespace().AddMembers(SerializerGenerator.GenerateClass(context.Contexts[0])));

        }
    }
}
