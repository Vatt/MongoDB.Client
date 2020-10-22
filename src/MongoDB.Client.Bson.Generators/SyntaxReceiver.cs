using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {

            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (classDeclarationSyntax.AttributeLists == null)
                {
                    return;
                }
                foreach (var attrList in classDeclarationSyntax.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        if (((IdentifierNameSyntax)attr.Name).Identifier.Text.Equals("BsonSerializable"))
                        {
                            Candidates.Add(classDeclarationSyntax);

                        }
                    }

                }

            }
            else if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax)
            {
                if (structDeclarationSyntax.AttributeLists == null)
                {
                    return;
                }
                foreach (var attrList in structDeclarationSyntax.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        if (((IdentifierNameSyntax)attr.Name).Identifier.Text.Equals("BsonSerializable"))
                        {
                            Candidates.Add(structDeclarationSyntax);

                        }
                    }

                }

            }
        }
    }
}
