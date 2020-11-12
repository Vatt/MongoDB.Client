using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class EnumTryParseMethodDeclaration : SimpleTryParseMethodDeclaration
    {
        internal static SyntaxToken VarToken = SF.Identifier("enumValue");
        internal static IdentifierNameSyntax VarId = SF.IdentifierName("enumValue");
        public EnumTryParseMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
        {
        }

        public override BlockSyntax GenerateMethodBody()
        {
            foreach (var attr in ClassSymbol.GetAttributes())
            {
                if (attr.AttributeClass.Name.Equals("BsonEnumSerializable"))
                {
                    return ((int)attr.ConstructorArguments[0].Value) switch
                    {
                        1 => StringRepresentation(),
                        2 or 3 => NumericRepresentation((int)attr.ConstructorArguments[0].Value),
                        _ => default,
                    };
                }
            }
            return default;
        }
        private BlockSyntax NumericRepresentation(int type)
        {
            SyntaxToken castType = type == 2 ? SF.Token(SyntaxKind.IntKeyword) : SF.Token(SyntaxKind.LongKeyword);
            IdentifierNameSyntax MethodId = type == 2 ? SF.IdentifierName("TryGetInt32") : SF.IdentifierName("TryGetInt64");
            CastExpressionSyntax castExpr = SF.CastExpression(SF.IdentifierName(SF.Identifier(ClassSymbol.ToString())), SF.IdentifierName("enumValue"));
            var invocation = SG.InvocationExpr(
                                Basics.ReaderInputVariableIdentifier,
                                MethodId,
                                SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), SF.DeclarationExpression(SF.PredefinedType(castType), SF.SingleVariableDesignation(SF.Identifier("enumValue")))));
                                //SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), castExpr));
            var defaultAssign = SG.SimpleAssignExpr(Basics.TryParseOutVariableIdentifier, SG.DefaultLiteralExpr());
            var assign = SF.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            Basics.TryParseOutVariableIdentifier,
                            castExpr);
            var readOp = SF.IfStatement(
                            condition: SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, invocation),
                            statement: SF.Block(SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
            return SF.Block(
                    SF.ExpressionStatement(defaultAssign),
                    readOp,
                    SF.ExpressionStatement(assign),
                    SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.TrueLiteralExpression))); 
        }
        public BlockSyntax StringRepresentation()
        {
            var variableDecl = SF.DeclarationExpression(SG.ReadOnlySpanByte(), SF.SingleVariableDesignation(VarToken));
            //var invocation = SG.InvocationExpr(Basics.ReaderInputVariableIdentifier,SF.IdentifierName("TryGetStringAsSpan"),SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), variableDecl));
            var invocation = SG.TryGetStringAsSpan(variableDecl);
            var defaultAssigment = SG.SimpleAssignExpr(Basics.TryParseOutVariableIdentifier, SG.DefaultLiteralExpr());
            return SF.Block(
                      SF.ExpressionStatement(defaultAssigment),
                      SF.ExpressionStatement(invocation))
                     .AddStatements(OperationsList.CreateReadOperations(ClassSymbol, Members).Generate())
                     .AddStatements(SF.ParseStatement(@$"throw new ArgumentException($""{ClassSymbol.Name}.TryParse  with enum string value {{System.Text.Encoding.UTF8.GetString(enumValue)}}"");"));
        }

    }
}