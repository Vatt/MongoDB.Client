using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly EnumMemberDeclarationSyntax InitialEnumState = SF.EnumMemberDeclaration("Initial");
        private static readonly EnumMemberDeclarationSyntax MainLoopEnumState = SF.EnumMemberDeclaration("MainLoop");
        private static readonly EnumMemberDeclarationSyntax EndMarkerEnumState = SF.EnumMemberDeclaration("EndMarker");
        private static readonly SyntaxToken StatePropertyName = Identifier("State");
        private static readonly SyntaxToken StateVariableName = Identifier("state");
        private static readonly SyntaxToken TypedStateToken = Identifier("typedState");
        private static readonly string SerializerBaseTypeString = "MongoDB.Client.Bson.Serialization.SerializerStateBase";
        private static readonly SyntaxToken SerializerBaseTypeToken = Identifier(SerializerBaseTypeString);
        private static INamedTypeSymbol SerializerStateBaseNamedType => BsonSerializerGenerator.Compilation.GetTypeByMetadataName(SerializerBaseTypeString)!;
        private static readonly BaseListSyntax StateBase = SF.BaseList(SeparatedList(SF.SimpleBaseType(SF.ParseTypeName(SerializerBaseTypeString))));
        private static SyntaxToken GenerateMemberEnumStateName(MemberContext ctx) => Identifier($"{ctx.NameSym.Name}InProgress");
        private static SyntaxToken GenerateNameOfEnumStates(ContextCore ctx) => Identifier($"{ctx.Declaration.Name}States");
        public static TypeSyntax GenerateTypeNameOfEnumStates(ContextCore ctx) => SF.ParseTypeName($"{ctx.Declaration.Name}States");
        private static SyntaxToken GenetateStateName(ContextCore ctx) => Identifier($"{ctx.Declaration.Name}State");
        private static EnumDeclarationSyntax GenerateEnumOfStates(ContextCore ctx)
        {
            List<EnumMemberDeclarationSyntax> states = new();
            states.Add(InitialEnumState);
            states.Add(MainLoopEnumState);
            states.Add(EndMarkerEnumState);
            foreach(var member in ctx.Members)
            {
                var truetype = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (IsBsonSerializable(truetype))
                {
                    states.Add(SF.EnumMemberDeclaration(GenerateMemberEnumStateName(member)));
                }
            }
            return SF.EnumDeclaration(default, default, GenerateNameOfEnumStates(ctx), default, SeparatedList(states));
        }
        private static MemberDeclarationSyntax GenerateState(ContextCore ctx)
        {
            List<PropertyDeclarationSyntax> properties = new();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                properties.Add(SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword()),
                        type: SF.ParseTypeName(trueType.ToString()),
                        explicitInterfaceSpecifier: default,
                        identifier: member.AssignedVariableToken,
                        accessorList: default,
                        expressionBody: default,
                        initializer: default,
                        semicolonToken: SemicolonToken()));
            }
            return SF.ClassDeclaration(GenetateStateName(ctx))
                .WithBaseList(StateBase)
                .AddMembers(SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword()),
                        type: GenerateTypeNameOfEnumStates(ctx),
                        explicitInterfaceSpecifier: default,
                        identifier: StatePropertyName,
                        accessorList: default,
                        expressionBody: default,
                        initializer: default,
                        semicolonToken: SemicolonToken()))
                .AddMembers(properties.ToArray());
            ConstructorDeclarationSyntax GenerateStateConstructor()
            {
                return default;
            }
        }
        private static MemberDeclarationSyntax CreateMessageMethod(ContextCore ctx)
        {
            var result = new List<StatementSyntax>();
            result.Add(VarLocalDeclarationStatement(TypedStateToken, Cast(SF.ParseTypeName($"{ctx.Declaration.Name}State"), IdentifierName(StateVariableName))));
            if (ctx.HavePrimaryConstructor)
            {
                List<ArgumentSyntax> args = new();
                var assignments = new List<ExpressionStatementSyntax>();
                foreach (var member in ctx.Members)
                {
                    if (ctx.ConstructorParamsBinds.TryGetValue(member.NameSym, out var parameter))
                    {
                        args.Add(Argument(SimpleMemberAccess(TypedStateToken, member.AssignedVariableToken), NameColon(parameter)));
                    }
                    else
                    {
                        assignments.Add(
                            SimpleAssignExprStatement(
                                SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                                SimpleMemberAccess(TypedStateToken, member.AssignedVariableToken)));
                    }
                }

                var creation = VarLocalDeclarationStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration, args.ToArray()));
                result.Add(creation);
                result.AddRange(assignments);
            }
            else
            {
                result.Add(VarLocalDeclarationStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration)));
                foreach (var member in ctx.Members)
                {
                    result.Add(
                        SimpleAssignExprStatement(
                            SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                            SimpleMemberAccess(TypedStateToken, member.AssignedVariableToken)));
                }
            }
            return SF.MethodDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword(), StaticKeyword()),
                        explicitInterfaceSpecifier: default,
                        returnType: SF.ParseTypeName(ctx.Declaration.ToString()),
                        identifier: CreateMessageToken,
                        parameterList: ParameterList(Parameter(SF.ParseTypeName(SerializerBaseTypeString), StateVariableName)),
                        body: default,
                        constraintClauses: default,
                        expressionBody: default,
                        typeParameterList: default,
                        semicolonToken: default)
                    .WithBody(Block(
                        
                        result.ToArray(), 
                        ReturnStatement(TryParseOutVarToken)));
        }
    }
}
