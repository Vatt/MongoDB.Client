using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();
        public List<EnumDeclarationSyntax> Enums { get; } = new List<EnumDeclarationSyntax>();
        public void AddIfhaveBsonAttribute(BaseTypeDeclarationSyntax decl)
        {
            if (decl.AttributeLists == null)
            {
                return;
            }
            foreach (var attrList in decl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name is IdentifierNameSyntax identifier)
                    {
                        if (identifier.Identifier.Text.Equals("BsonSerializable"))
                        {
                            if (decl is EnumDeclarationSyntax enumdecl)
                            {
                                Enums.Add(enumdecl);
                            }
                            else
                            {
                                Candidates.Add(decl as TypeDeclarationSyntax);
                            }
                            
                        }
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
                case EnumDeclarationSyntax enumDecl:
                {
                    AddIfhaveBsonAttribute(enumDecl);
                    break; 
                }
                    //case RecordDeclarationSyntax recorddecl:
                    //    {
                    //        AddIfhaveBsonAttribute(recorddecl);
                    //        break;
                    //    }
            }
        }
    }
}
