using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class ClassDeclarationBase
    {
        protected ClassDeclarationSyntax ClassDeclatation;
        protected ClassDeclMeta classDecl;
        protected TryParseMethodDeclatationBase TryParseMethod;
        internal INamedTypeSymbol ClassSymbol => classDecl.ClassSymbol;
        internal ClassDeclMeta ClassDecl => classDecl;
        internal List<MemberDeclarationMeta> Members => classDecl.MemberDeclarations;
        public ClassDeclarationBase(ClassDeclMeta classdecl)
        {
            classDecl = classdecl;

        }
        public abstract MethodDeclarationSyntax DeclareTryParseMethod();
        public abstract MethodDeclarationSyntax DeclareWriteMethod();

        public abstract TypeSyntax GetTryParseMethodOutParameter();
        public abstract TypeArgumentListSyntax GetInterfaceParameters();

        public virtual TypeParameterListSyntax GetTypeParametersList()
        {
            var paramsList = new SeparatedSyntaxList<TypeParameterSyntax>();
            foreach (var param in classDecl.ClassSymbol.TypeParameters)
            {
                paramsList = paramsList.Add(SF.TypeParameter(param.Name));
            }
            return SF.TypeParameterList(paramsList);
        }
        public SeparatedSyntaxList<BaseTypeSyntax> GetBaseList()
        {
            SeparatedSyntaxList<BaseTypeSyntax> list = new SeparatedSyntaxList<BaseTypeSyntax>();
            return list.Add(SF.SimpleBaseType(SF.GenericName(Basics.SerializerInterface).WithTypeArgumentList(GetInterfaceParameters())));

        }
        public static ArrowExpressionClauseSyntax GenerateSpanNameValues(MemberDeclarationMeta memberdecl) //=> { byte, byte, byte, byte}
        {
            SeparatedSyntaxList<ExpressionSyntax> ArrayInitExpr(MemberDeclarationMeta memberdecl)
            {
                SeparatedSyntaxList<ExpressionSyntax> expr = new SeparatedSyntaxList<ExpressionSyntax>();
                foreach (var byteItem in Encoding.UTF8.GetBytes(memberdecl.StringBsonAlias))
                {
                    expr = expr.Add(Basics.NumberLiteral(byteItem));
                }
                return expr;
            }
            ArrayRankSpecifierSyntax ArrayRank(MemberDeclarationMeta memberdecl)
            {
                var bytes = Encoding.UTF8.GetBytes(memberdecl.StringBsonAlias);
                return SF.ArrayRankSpecifier().AddSizes(Basics.NumberLiteral(bytes.Length));
            }

            return SF.ArrowExpressionClause(
                    SF.ArrayCreationExpression(
                        SF.ArrayType(
                           SF.PredefinedType(SF.Token(SyntaxKind.ByteKeyword)),
                           SF.SingletonList<ArrayRankSpecifierSyntax>(ArrayRank(memberdecl))),
                        SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression, ArrayInitExpr(memberdecl))
                    )
                );
        }
        public SyntaxList<MemberDeclarationSyntax> GenerateStaticNamesSpans()
        {
            SyntaxList<MemberDeclarationSyntax> list = new SyntaxList<MemberDeclarationSyntax>();
            foreach (var memberdecl in classDecl.MemberDeclarations)
            {
                list = list.Add(
                   SF.PropertyDeclaration(
                       attributeLists: default,
                       modifiers: Basics.PrivateStatic,
                       type: SF.ParseTypeName("ReadOnlySpan<byte>"),
                       explicitInterfaceSpecifier: default,
                       identifier: Basics.GenerateReadOnlySpanNameSyntaxToken(ClassSymbol, memberdecl),
                       accessorList: default,
                       expressionBody: GenerateSpanNameValues(memberdecl),
                       initializer: default,
                       semicolonToken: SF.Token(SyntaxKind.SemicolonToken)
                    )
                );
            }
            return list;
        }
        public abstract ClassDeclarationSyntax Generate();
    }
}
