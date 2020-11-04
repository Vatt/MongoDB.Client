using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class OperationsList
    {
        private List<OperationBase> _operations;
        private OperationsList(List<OperationBase> operations)
        {
            _operations = operations;
        }
        public static OperationsList CreateReadOperations(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members)
        {
            var operations = new List<OperationBase>();
            foreach (var member in members)
            {
                if (member.IsGenericList)
                {
                    operations.Add(new InLoopArrayReadOperation(classSymbol, member));
                }
                else if (member.IsProperty)
                {
                    operations.Add(new InLoopPropertyReadOperation(classSymbol, member));
                }
                else
                {
                    operations.Add(new InLoopFieldReadOperation(classSymbol, member));
                }
            }
            return new OperationsList(operations);
        }
        public static OperationsList CreateWriteOperations(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members)
        {
            var operations = new List<OperationBase>();
            foreach (var member in members)
            {
                if (member.IsGenericList)
                {
                    operations.Add(new ArrayWriteOperation(classSymbol, member));
                }
                else
                {
                    operations.Add(new SimpleWriteOperation(classSymbol, member));
                }


            }
            return new OperationsList(operations);
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
