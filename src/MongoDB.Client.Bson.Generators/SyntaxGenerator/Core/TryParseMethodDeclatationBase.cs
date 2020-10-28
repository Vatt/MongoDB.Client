using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class TryParseMethodDeclatationBase
    {
        protected ClassDeclarationBase ClassDecl;
        public INamedTypeSymbol ClassSymbol => ClassDecl.ClassSymbol;
        public TryParseMethodDeclatationBase(ClassDeclarationBase classdecl)
        {
            ClassDecl = classdecl;
        }
        public abstract TypeSyntax GetParseMethodOutParameter();

        public abstract ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier();

        //GeneratorBasics.SimpleMemberAccess(GeneratorBasics.ReaderInputVariableIdentifier,SF.IdentifierName("TryGetInt32"))
        public virtual BlockSyntax GenerateMethodBody()
        {
            var list = new OperationsList(ClassSymbol, ClassDecl.Members);
            var whileStatement = new SyntaxList<StatementSyntax>()
                                    .Add(SF.ParseStatement("if (!reader.TryGetByte(out var bsonType)) { return false; }"))
                                    .Add(SF.ParseStatement("if (!reader.TryGetCStringAsSpan(out var bsonName)) { return false; } "))
                                    .AddRange(list.Generate())
                                    .Add(SF.ParseStatement(@$"throw new ArgumentException($""{ClassSymbol.Name}.TryParse  with bson type number {{bsonType}}"");"));
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

        public ParameterListSyntax GetTryParseParameterList()
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
        public MethodDeclarationSyntax DeclareTryParseMethod()
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
