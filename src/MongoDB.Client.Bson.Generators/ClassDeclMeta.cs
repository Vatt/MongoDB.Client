using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    internal class ClassDeclMeta
    {
        public INamedTypeSymbol ClassSymbol { get; set; }
        public string StringNamespace => ClassSymbol.ContainingNamespace.ToString();
        public List<MemberDeclarationMeta> MemberDeclarations { get; set; }
        public string FullName => ClassSymbol.ToString();
        public ClassDeclMeta(INamedTypeSymbol classSymbol)
        {
            MemberDeclarations = new List<MemberDeclarationMeta>();
            ClassSymbol = classSymbol;
        }
    }
}
