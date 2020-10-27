using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class OperationsList
    {
        private List<InLoopFieldOperation> _operations;
        public OperationsList(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members)
        {
            _operations = new List<InLoopFieldOperation>();
            foreach (var member in members)
            {
                _operations.Add(new InLoopFieldOperation(classSymbol, member));
            }
        }
        public StatementSyntax[] Generate()
        {
            var statements = new StatementSyntax[_operations.Count];
            for (int index = 0; index < _operations.Count; index++)
            {
                statements[index] = _operations[index].Generate();
            }
            return statements;
        }
    }
}
