using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();
        public void AddIfhaveBsonAttribute(TypeDeclarationSyntax decl)
        {
            if (decl.AttributeLists == null)
            {
                return;
            }
            foreach (var attrList in decl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (((IdentifierNameSyntax)attr.Name).Identifier.Text.Equals("BsonSerializable"))
                    {
                        Candidates.Add(decl);

                    }
                }

            }
        }
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case ClassDeclarationSyntax classdecl:
                    {
                        AddIfhaveBsonAttribute(classdecl);
                        break;
                    }
                case StructDeclarationSyntax structdecl:
                    {
                        AddIfhaveBsonAttribute(structdecl);
                        break;
                    }
                case RecordDeclarationSyntax recorddecl:
                    {
                        AddIfhaveBsonAttribute(recorddecl);
                        break;
                    }
            }
        }
    }
}
