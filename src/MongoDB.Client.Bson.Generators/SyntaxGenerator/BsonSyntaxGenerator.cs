using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    partial class BsonSyntaxGenerator
    {
        private CompilationUnitSyntax _root;
        private NamespaceDeclarationSyntax _namespace;
        private ClassDeclarationSyntax _class;
        private ClassDeclMeta _decl;
        private ClassDeclarationBase cd;
        private BsonSyntaxGenerator(ClassDeclMeta decl)
        {
            _decl = decl;
            _root = SF.CompilationUnit();
            _root = _root.AddUsings(
                SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Reader")),
                SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Serialization")),
                SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Document")),
                SF.UsingDirective(SF.ParseName("System")),
                SF.UsingDirective(SF.ParseName("System.Collections.Generic")),
                SF.UsingDirective(SF.ParseName("MongoDB.Client")),
                SF.UsingDirective(SF.ParseName(decl.StringNamespace))
            );
            _namespace = SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated"));
            
            if (decl.ClassSymbol.IsGenericType)
            {
                cd = new GenericClassDeclaration(decl);
            }
            else
            {
                cd = new SimpleClassDeclaration(decl);
            }
        }
        
        public void Build()
        {
            _root = _root.AddMembers(
                cd.Build()
                ); 
        }
        public override string ToString()
        {
            return _root.NormalizeWhitespace().ToString();
        }
        public static BsonSyntaxGenerator Create(ClassDeclMeta decl) => new BsonSyntaxGenerator(decl);
    }
}
