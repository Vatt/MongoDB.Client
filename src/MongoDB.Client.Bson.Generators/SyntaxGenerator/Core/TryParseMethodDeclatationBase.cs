using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class TryParseMethodDeclatationBase
    {
        protected ClassDeclarationBase ClassDecl;
        protected MethodDeclarationSyntax _parseMethod;
        public INamedTypeSymbol ClassSymbol => ClassDecl.ClassSymbol;
        public TryParseMethodDeclatationBase(ClassDeclarationBase classdecl)
        {
            ClassDecl = classdecl;
        }
        public abstract TypeSyntax GetParseMethodOutParameter();
        //public abstract ParameterListSyntax GetTryParseParameterList();
        public abstract ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier();

        public ParameterListSyntax GetTryParseParameterList()
        {
            return SF.ParameterList()
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.RefKeyword)),
                        identifier: GeneratorBasics.ReaderInputVariable,
                        type: GeneratorBasics.ReaderInputVariableType,
                        @default: default))
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.OutKeyword)),
                        identifier: GeneratorBasics.TryParseOutputVariable,
                        type: GetParseMethodOutParameter(),
                        @default: default));
        }
        public MethodDeclarationSyntax DeclareTryParseMethod()
        {
            _parseMethod = SF.MethodDeclaration(
                attributeLists: default,
                modifiers: GeneratorBasics.Public,
                explicitInterfaceSpecifier: ExplicitInterfaceSpecifier(),
                returnType: SF.ParseTypeName("bool"),
                identifier: SF.Identifier($"TryParse"),
                parameterList: GetTryParseParameterList(),
                body: default,
                constraintClauses: default,
                expressionBody: default,
                typeParameterList: default,
                semicolonToken: SF.Token(SyntaxKind.SemicolonToken)
                );
            _parseMethod = _parseMethod.WithBody(SF.Block(new ReadOperation(ClassSymbol, ClassDecl.Members[0]).Build()));
            return _parseMethod;
        }
    }
}
