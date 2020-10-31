using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class TryParseMethodDeclatationBase : MethodDeclarationBase
    {

        public TryParseMethodDeclatationBase(INamedTypeSymbol classSymbol, List<MemberDeclarationMeta> members) : base(classSymbol, members)
        {
        }
        public abstract TypeSyntax GetParseMethodOutParameter();

        public override BlockSyntax GenerateMethodBody()
        {
            var whileStatement = new SyntaxList<StatementSyntax>()
                                    .Add(SF.ParseStatement("if (!reader.TryGetByte(out var bsonType)) { return false; }"))
                                    .Add(SF.ParseStatement("if (!reader.TryGetCStringAsSpan(out var bsonName)) { return false; } "))
                                    .AddRange(OperationsList.CreateReadOperations(ClassSymbol, Members).Generate())
                                    .Add(SF.ParseStatement(@$"throw new ArgumentException($""{ClassSymbol.Name}.TryParse  with bson type number {{bsonType}} and name {{System.Text.Encoding.UTF8.GetString(bsonName)}}"");"));
            return SF.Block(
                SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, Basics.TryParseOutVariableIdentifier, Basics.ObjectCreationWitoutArgs(ClassSymbol))),
                SF.ParseStatement("if (!reader.TryGetInt32(out var docLength)) { return false; }"),
                SF.ParseStatement("var unreaded = reader.Remaining + sizeof(int);"),
                SF.WhileStatement(
                    attributeLists: default,
                    condition: SF.ParseExpression("unreaded - reader.Remaining < docLength - 1"),
                    statement: SF.Block(whileStatement)),
                SF.ParseStatement(@$"if ( !reader.TryGetByte(out var endMarker)){{ return false; }}"),
                SF.ParseStatement($@"if (endMarker != '\x00'){{ throw new ArgumentException(""{ClassSymbol.Name}GeneratedSerializator.TryParse End document marker missmatch""); }}"),
                SF.ParseStatement("return true;")
                );

        }

        public virtual ParameterListSyntax GetTryParseParameterList()
        {
            return SF.ParameterList()
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.RefKeyword)),
                        identifier: Basics.ReaderInputVariable,
                        type: Basics.ReaderInputVariableType,
                        @default: default))
                    .AddParameters(SF.Parameter(
                        attributeLists: default,
                        modifiers: new SyntaxTokenList().Add(SF.Token(SyntaxKind.OutKeyword)),
                        identifier: Basics.TryParseOutputVariable,
                        type: GetParseMethodOutParameter(),
                        @default: default));
        }
        public override MethodDeclarationSyntax DeclareMethod()
        {

            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    explicitInterfaceSpecifier: ExplicitInterfaceSpecifier(),
                    returnType: SF.ParseTypeName("bool"),
                    identifier: SF.Identifier($"TryParse"),
                    parameterList: GetTryParseParameterList(),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(GenerateMethodBody());
        }
    }
}
