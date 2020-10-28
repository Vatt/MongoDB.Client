using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class OperationsList
    {
        private List<OperationBase> _operations;
        public OperationsList(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members)
        {
            _operations = new List<OperationBase>();
            foreach (var member in members)
            {
                if (member.IsGenericList)
                {
                    _operations.Add(new InLoopArrayReadOperation(classSymbol, member));
                }
                else if (member.IsProperty)
                {
                    _operations.Add(new InLoopPropertyReadOperation(classSymbol, member));
                }
                else
                {
                    _operations.Add(new InLoopFieldReadOperation(classSymbol, member));
                }
                
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
