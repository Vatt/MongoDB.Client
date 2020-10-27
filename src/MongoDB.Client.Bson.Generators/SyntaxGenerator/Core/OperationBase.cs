using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class OperationBase
    {
        public INamedTypeSymbol ClassSymbol { get; }
        public MemberDeclarationMeta MemberDecl { get; }

        public OperationBase(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl)
        {
            ClassSymbol = classsymbol;
            MemberDecl = memberdecl;
        }
        public abstract StatementSyntax Generate();
    }
}
