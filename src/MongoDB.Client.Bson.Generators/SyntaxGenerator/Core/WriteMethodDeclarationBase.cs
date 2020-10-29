using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class WriteMethodDeclarationBase : MethodDeclarationBase
    {
        public WriteMethodDeclarationBase(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members) : base(classSymbol, members)
        {
            
        }
        public abstract TypeSyntax GetWriteMethodInParameter();
        public virtual ParameterListSyntax GetWriteParameterList()
        {
            return SF.ParameterList()
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.RefKeyword)),
                        identifier: Basics.WriterInputVariable,
                        type: Basics.WriterInputVariableType,
                        @default: default))
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.InKeyword)),
                        identifier: Basics.WriteInputInVariable,
                        type: GetWriteMethodInParameter(),
                        @default: default));
        }
        public override MethodDeclarationSyntax DeclareMethod()
        {
            return SF.MethodDeclaration(
                         attributeLists: default,
                         modifiers: default,
                         explicitInterfaceSpecifier: ExplicitInterfaceSpecifier(),
                         returnType: SF.ParseTypeName("void"),
                         identifier: SF.Identifier($"Write"),
                         parameterList: GetWriteParameterList(),
                         body: default,
                         constraintClauses: default,
                         expressionBody: default,
                         typeParameterList: default,
                         semicolonToken: default)
                     .WithBody(GenerateMethodBody());
        }
    }
}
