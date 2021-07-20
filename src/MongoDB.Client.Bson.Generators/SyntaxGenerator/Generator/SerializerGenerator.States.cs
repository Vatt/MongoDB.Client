using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static INamedTypeSymbol SerializerStateBaseNamedType => BsonSerializerGenerator.Compilation.GetTypeByMetadataName(SerializerBaseStateTypeString)!;
        private static readonly BaseListSyntax StateBase = SF.BaseList(SeparatedList(SF.SimpleBaseType(SF.ParseTypeName(SerializerBaseStateTypeString))));
        private static SyntaxToken MemberEnumStateNameToken(MemberContext ctx) => Identifier($"{ctx.NameSym.Name}InProgress");
        private static SyntaxToken NameOfEnumStatesToken(ContextCore ctx) => Identifier($"{ctx.Declaration.Name}States");
        private static ExpressionSyntax EnumStateFromContextAccess(ContextCore ctx, MemberContext member) => SimpleMemberAccess(NameOfEnumStatesToken(ctx), MemberEnumStateNameToken(member));
        public static TypeSyntax TypeNameOfEnumStates(ContextCore ctx) => SF.ParseTypeName($"{ctx.Declaration.Name}States");
        private static SyntaxToken StateNameToken(ContextCore ctx) => Identifier($"{ctx.Declaration.Name}State");
        private static TypeSyntax StateNameType(ContextCore ctx) => SF.ParseTypeName($"{ctx.Declaration.Name}State");
        private static StatementSyntax VarLocalTypedStateDeclFromOrigimalState(ContextCore ctx) => VarLocalDeclarationStatement(TypedStateToken, Cast(StateNameType(ctx), IdentifierName(StateToken)));
        private static MemberAccessExpressionSyntax TypedStateMemberAccess(MemberContext ctx) => SimpleMemberAccess(TypedStateToken, ctx.AssignedVariableToken);
        private static MemberAccessExpressionSyntax TypedStateStateAccess => SimpleMemberAccess(TypedStateToken, Identifier("State"));
        private static StatementSyntax TypedStateStateAccessStatement(MemberContext ctx) => Statement(TypedStateStateAccess);
        private static StatementSyntax TypedStateMemberAccessStatement(MemberContext ctx) => Statement(TypedStateMemberAccess(ctx));
        private static MemberAccessExpressionSyntax StateMemberAccess(MemberContext ctx) => SimpleMemberAccess(StateToken, ctx.AssignedVariableToken);
        private static readonly SyntaxToken NameOfEnumCollectionStatesToken = Identifier($"CollectionStates");
        private static SyntaxToken NameOfCollectionState(ITypeSymbol type) => Identifier($"{UnwrapTypeName(type)}State");
        private static ExpressionSyntax CreateMessageFromState(MemberContext ctx)
        {
            var trueType = ExtractTypeFromNullableIfNeed(ctx.TypeSym);
            return InvocationExpr(IdentifierName(trueType.ToString()), CreateMessageToken, Argument(TypedStateMemberAccess(ctx)));
        }
        private static ExpressionSyntax CollectionStateMemberAccess(MemberContext ctx) => SimpleMemberAccess(TypedStateToken, ctx.AssignedVariableToken, CollectionToken);
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
                    states.Add(SF.EnumMemberDeclaration(MemberEnumStateNameToken(member)));
                }
            }
            return SF.EnumDeclaration(default, default, NameOfEnumStatesToken(ctx), default, SeparatedList(states));
        }
        private static MemberDeclarationSyntax GenerateState(ContextCore ctx)
        {
            List<PropertyDeclarationSyntax> properties = new();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                TypeSyntax propType;
                if (IsBsonSerializable(trueType))
                {
                    propType = SF.ParseTypeName(SerializerStateBaseNamedType.ToString());
                }
                else if (IsCollection(trueType))
                {
                    propType = IdentifierName(NameOfCollectionState(trueType));
                }
                else
                {
                    propType = SF.ParseTypeName(trueType.ToString());
                }
                properties.Add(SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword()),
                        type: propType,
                        explicitInterfaceSpecifier: default,
                        identifier: member.AssignedVariableToken,
                        accessorList: default,
                        expressionBody: default,
                        initializer: default,
                        semicolonToken: SemicolonToken()));
            }
            return SF.ClassDeclaration(StateNameToken(ctx))
                .WithBaseList(StateBase)
                .WithMembers(new(GenerateCollectionStatesIfNeed(ctx)))
                .AddMembers(SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword()),
                        type: TypeNameOfEnumStates(ctx),
                        explicitInterfaceSpecifier: default,
                        identifier: StatePropertyNameToken,
                        accessorList: default,
                        expressionBody: default,
                        initializer: default,
                        semicolonToken: SemicolonToken()))
                .AddMembers(properties.ToArray())
                .AddMembers(GenerateStateConstructor());
            ConstructorDeclarationSyntax GenerateStateConstructor()
            {
                var statements = new List<StatementSyntax>();
                statements.Add(SimpleAssignExprStatement(StatePropertyNameToken, SimpleMemberAccess(TypeNameOfEnumStates(ctx), InitialEnumStateToken)));
                foreach (var member in ctx.Members)
                {
                    statements.Add(SimpleAssignExprStatement(member.AssignedVariableToken, DefaultLiteralExpr()));
                }
                return SF.ConstructorDeclaration(StateNameToken(ctx))
                    .WithModifiers(new(PublicKeyword()))
                    .WithBody(Block(statements));
            }
        }
        private static MemberDeclarationSyntax CreateMessageMethod(ContextCore ctx)
        {
            var result = new List<StatementSyntax>();
            result.Add(VarLocalTypedStateDeclFromOrigimalState(ctx));
            if (ctx.HavePrimaryConstructor)
            {
                List<ArgumentSyntax> args = new();
                var assignments = new List<ExpressionStatementSyntax>();
                foreach (var member in ctx.Members)
                {
                    var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                    if (ctx.ConstructorParamsBinds.TryGetValue(member.NameSym, out var parameter))
                    {
                        if(IsBsonSerializable(trueType))
                        {
                            args.Add(Argument(CreateMessageFromState(member), NameColon(parameter)));
                        }
                        else if (IsCollection(trueType))
                        {
                            args.Add(Argument(CollectionStateMemberAccess(member), NameColon(parameter)));
                        }
                        else
                        {
                            args.Add(Argument(TypedStateMemberAccess(member), NameColon(parameter)));
                        }
                        
                    }
                    else
                    {
                        if (IsBsonSerializable(trueType))
                        {
                            assignments.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), CreateMessageFromState(member)));
                        }
                        else if (IsCollection(trueType))
                        {
                            assignments.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), CollectionStateMemberAccess(member)));
                        }
                        else
                        {
                            assignments.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), TypedStateMemberAccess(member)));
                        }
                        
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
                    var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                    if (IsBsonSerializable(trueType))
                    {
                        result.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), CreateMessageFromState(member)));
                    }
                    else if (IsCollection(trueType))
                    {
                        result.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), CollectionStateMemberAccess(member)));
                    }
                    else
                    {
                        result.Add(SimpleAssignExprStatement(SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)), TypedStateMemberAccess(member)));
                    }
                    
                }
            }
            return SF.MethodDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword(), StaticKeyword()),
                        explicitInterfaceSpecifier: default,
                        returnType: SF.ParseTypeName(ctx.Declaration.ToString()),
                        identifier: CreateMessageToken,
                        parameterList: ParameterList(Parameter(SF.ParseTypeName(SerializerBaseStateTypeString), StateToken)),
                        body: default,
                        constraintClauses: default,
                        expressionBody: default,
                        typeParameterList: default,
                        semicolonToken: default)
                    .WithBody(Block(result.ToArray(), ReturnStatement(TryParseOutVarToken)));
        }
        private static IEnumerable<MemberDeclarationSyntax> GenerateCollectionStatesIfNeed(ContextCore ctx)
        {
            List<MemberDeclarationSyntax> result = new();
            HashSet<ITypeSymbol> alreadyCreated = new();
            
            foreach (var member in ctx.Members.Where(ctx => IsCollection(ctx.TypeSym)))
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (alreadyCreated.Contains(trueType))
                {
                    continue;
                }
                result.Add(
                SF.StructDeclaration(NameOfCollectionState(trueType))
                  .AddModifiers(PublicKeyword())
                  .AddMembers(SF.FieldDeclaration(
                                    attributeLists: default,
                                    modifiers: new(PublicKeyword()),
                                    declaration: SF.VariableDeclaration(NullableIntType, SeparatedList(SF.VariableDeclarator(DocLenToken))),
                                    semicolonToken: SemicolonToken()),
                              SF.FieldDeclaration(
                                    attributeLists: default,
                                    modifiers: new(PublicKeyword()),
                                    declaration: SF.VariableDeclaration(IntPredefinedType(), SeparatedList(SF.VariableDeclarator(ConsumedToken))),
                                    semicolonToken: SemicolonToken()),
                              SF.FieldDeclaration(
                                    attributeLists: default,
                                    modifiers: new(PublicKeyword()),
                                    declaration: SF.VariableDeclaration(ConstructCollectionType(trueType), SeparatedList(SF.VariableDeclarator(CollectionToken))),
                                    semicolonToken: SemicolonToken()))

                        );
                alreadyCreated.Add(trueType);   
            }
            if (result.Count > 0)
            {
                result.Add(SF.EnumDeclaration(default, default, NameOfEnumCollectionStatesToken, default, SeparatedList(InitialEnumState, EndMarkerEnumState, InProgressEnumState)));
            }
            return result;
        }
    }
}
