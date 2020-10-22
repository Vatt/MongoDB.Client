using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    internal class ClassDecl
    {
        public INamedTypeSymbol ClassSymbol { get; set; }
        public string StringNamespace
        {
            get
            {
                return ClassSymbol.ContainingNamespace.ToString();
            }
        }
        public List<MemberDeclarationInfo> MemberDeclarations { get; set; }
        public ClassDecl(INamedTypeSymbol classSymbol)
        {
            MemberDeclarations = new List<MemberDeclarationInfo>();
            ClassSymbol = classSymbol;
        }
        public string GetNamespace()
        {
            return ClassSymbol.ContainingNamespace.ToString();
        }
    }
}
