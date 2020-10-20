using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidates { get; } = new List<ClassDeclarationSyntax>();
 
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if(classDeclarationSyntax.AttributeLists == null)
                {
                    return;
                }
                var info = new ClassMapInfo();
                foreach(var attrList in classDeclarationSyntax.AttributeLists)
                {
                    foreach(var attr in attrList.Attributes)
                    {
                        if (((IdentifierNameSyntax)attr.Name).Identifier.Text.Equals("BsonSerializable"))
                        {
                            Candidates.Add(classDeclarationSyntax);

                        }
                    }
         
                }
                
            }
        }
    }
}
