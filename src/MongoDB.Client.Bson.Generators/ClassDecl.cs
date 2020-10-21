using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

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
