using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<GenerationContext> Contexts;
        public readonly GeneratorExecutionContext GeneratorExecutionContext;
        public MasterContext(List<ClassDeclMeta> meta, GeneratorExecutionContext ctx)
        {
            Contexts = new List<GenerationContext>();
            GeneratorExecutionContext = ctx;
            foreach (var item in meta)
            {
                Contexts.Add(new GenerationContext(this, item));
            }
        }
    }
    internal class GenerationContext
    {
        internal IdentifierNameSyntax BsonReaderId => SF.IdentifierName("reader");
        internal IdentifierNameSyntax BsonWriterId => SF.IdentifierName("writer");
        internal IdentifierNameSyntax TryParseOutVar => SF.IdentifierName("message");
        internal IdentifierNameSyntax WriterInputVar => SF.IdentifierName("message");
        
        internal readonly MasterContext Root;
        internal readonly INamedTypeSymbol Declaration;
        internal readonly List<MemberGenerationContext> Members;

        internal readonly List<ITypeSymbol>? GenericArgs;

        public GenerationContext(MasterContext root, ClassDeclMeta classdecl)
        {
            Root = root;
            Declaration = classdecl.ClassSymbol;
            Members = new List<MemberGenerationContext>();
            if (!Declaration.TypeArguments.IsEmpty)
            {
                GenericArgs = new List<ITypeSymbol>(Declaration.TypeArguments.Length);
                foreach (var typeArgument in Declaration.TypeArguments)
                {
                    GenericArgs.Add(typeArgument);
                }
            }

            foreach (var member in classdecl.MemberDeclarations)
            {
                Members.Add(new MemberGenerationContext(this, member));
            }
        }
    }
    internal class MemberGenerationContext
    {
        internal GenerationContext Root { get; }
        internal readonly ISymbol NameSym;
        internal readonly INamedTypeSymbol TypeSym;
        internal readonly string StringBsonAlias;
        internal readonly List<ITypeSymbol>? TypeGenericArgs;

        public MemberGenerationContext(GenerationContext root, MemberDeclarationMeta memberdecl)
        {
            Root = root;
            NameSym = memberdecl.DeclSymbol;
            TypeSym = memberdecl.DeclType;
            StringBsonAlias = memberdecl.StringBsonAlias;
            
            if (!TypeSym.TypeArguments.IsEmpty)
            {
                TypeGenericArgs = new List<ITypeSymbol>(TypeSym.TypeArguments.Length);
                foreach (var typeArgument in TypeSym.TypeArguments)
                {
                    TypeGenericArgs.Add(typeArgument);
                }
            }
        }
    }
}