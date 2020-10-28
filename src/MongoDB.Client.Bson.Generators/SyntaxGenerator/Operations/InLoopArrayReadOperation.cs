using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class InLoopArrayReadOperation : OperationBase
    {
        private string _variadleIdentifier => "value";
        public InLoopArrayReadOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        IfStatementSyntax GenerateRead()
        {
            ITypeSymbol type = MemberDecl.DeclType;
            if (MemberDecl.DeclType.Name.Equals("Nullable"))
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            if (MemberDecl.IsGenericList)
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            ReadsMap.TryGetValue(type/*MemberDecl.DeclType*/, out var readOp);
            readOp.WithVariableDeclaration(_variadleIdentifier);
            return SF.IfStatement(
                condition: SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, readOp.Generate()),
                statement: SF.Block(SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
        }
        IfStatementSyntax GenerateIfNameEqualsStatement()
        {
            var whileStatement = new SyntaxList<StatementSyntax>()
                        .Add(SF.ParseStatement("if (!reader.TryGetCStringAsSpan(out var index)) { return false; }"))
                        .Add(GenerateRead())
                        .Add(SF.ParseStatement($"message.{MemberDecl.DeclSymbol.Name}.Add(value);"));
            return SF.IfStatement(
                    condition: SF.InvocationExpression(
                                    expression: Basics.SimpleMemberAccess(Basics.TryParseBsonNameIdentifier, SF.IdentifierName("SequenceEqual")),
                                    argumentList: Basics.Arguments(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl))),
                    statement: SF.Block(GenerateIfBsonTypeNull(),
                                        SF.ParseStatement($"message.{MemberDecl.DeclSymbol.Name} = new List<{MemberDecl.GenericType.Name}>();"),
                                        SF.ParseStatement($"if (!reader.TryGetInt32(out var arrayDocLength)) {{ return false; }}"),
                                        SF.ParseStatement($"var arrayUnreaded = reader.Remaining + sizeof(int);"),
                                        SF.WhileStatement(
                                            attributeLists: default,
                                            condition: SF.ParseExpression("unreaded - reader.Remaining < docLength - 1"),
                                            statement: SF.Block(whileStatement)),
                                        SF.ParseStatement("if ( !reader.TryGetByte(out var arrayEndMarker)){ return false; }"),
                                        SF.ParseStatement(@$"if (arrayEndMarker != '\x00'){{ throw new ArgumentException($""{ClassSymbol.Name}GeneratedSerializer.TryParse End document marker missmatch"");}}"),
                                        SF.ContinueStatement()));
        }
        public override StatementSyntax Generate()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
