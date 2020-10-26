using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    internal class ClassDeclMeta
    {
        public INamedTypeSymbol ClassSymbol { get; set; }
        public string StringNamespace
        {
            get
            {
                return ClassSymbol.ContainingNamespace.ToString();
            }
        }
        public List<MemberDeclarationMeta> MemberDeclarations { get; set; }
        public ClassDeclMeta(INamedTypeSymbol classSymbol)
        {
            MemberDeclarations = new List<MemberDeclarationMeta>();
            ClassSymbol = classSymbol;
        }
    }
}
