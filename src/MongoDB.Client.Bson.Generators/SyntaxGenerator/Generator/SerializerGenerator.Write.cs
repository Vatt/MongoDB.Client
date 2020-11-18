using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static MethodDeclarationSyntax WriteMethod(ClassContext ctx)
        {
            var decl = ctx.Declaration;            
            
            var parameters = ParameterList(RefParameter(ctx.BsonReaderType, ctx.BsonReaderToken ),
                                           OutParameter(TypeFullName(ctx.Declaration), ctx.TryParseOutVarToken));
 
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    explicitInterfaceSpecifier: SF.ExplicitInterfaceSpecifier(GenericName(SerializerToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: SF.Identifier("Write"),
                    parameterList: parameters,
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(SF.Block());
        }        
    }
}