using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class EnumWriteMethodDeclaration : SimpleWriteMethodDeclaration
    {
        public EnumWriteMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
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
        BlockSyntax NumericRepresentation(int type)
        {
            CastExpressionSyntax castExpr = type == 2 ? SG.CastToInt(SF.IdentifierName("message")) : SG.CastToLong(SF.IdentifierName("message"));
            var invocation = type == 2 ? SG.WriteInt32(castExpr) : SG.WriteInt64(castExpr);
            return SF.Block(SF.ExpressionStatement(invocation));
        }
        BlockSyntax StringRepresentation()
        {
            return SF.Block(OperationsList.CreateWriteOperations(ClassSymbol, Members).Generate());
        }
    }
}
