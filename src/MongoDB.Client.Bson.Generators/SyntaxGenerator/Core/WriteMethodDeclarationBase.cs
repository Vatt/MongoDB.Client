using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class WriteMethodDeclarationBase : MethodDeclarationBase
    {
        public WriteMethodDeclarationBase(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members) : base(classSymbol, members)
        {

        }
    }
}
