using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class EnumReadOperation : OperationBase
    {
        public EnumReadOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {
        }

        public override StatementSyntax Generate()
        {
            throw new NotImplementedException();
        }
    }
}
