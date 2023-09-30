using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators
{

    [Generator(LanguageNames.CSharp)]
    public partial class BsonGenerator : IIncrementalGenerator
    {
        public static SourceProductionContext Context;

        public static Compilation Compilation;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif
            var declarations = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Where(static decl => decl != null);
            IncrementalValueProvider<(Compilation, ImmutableArray<ContextCore>)> compilationAndDeclarations = context.CompilationProvider.Combine(declarations.Collect());
            context.RegisterSourceOutput(compilationAndDeclarations, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }
        private bool Predicate(SyntaxNode node, CancellationToken token)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl when classDecl.AttributeLists.Count > 0: return true;
                case StructDeclarationSyntax structDecl when structDecl.AttributeLists.Count > 0: return true;
                case RecordDeclarationSyntax recordDecl when recordDecl.AttributeLists.Count > 0: return true;
                default: return false;
            }
        }


        private ContextCore Transform(GeneratorSyntaxContext context, CancellationToken token)
        {
            Compilation = context.SemanticModel.Compilation;
            var model = context.SemanticModel;


            if (model.GetDeclaredSymbol(context.Node) is INamedTypeSymbol symbol)
            {
                foreach (var attr in symbol.GetAttributes())
                {
                    if (attr.AttributeClass!.Equals(BsonSerializableAttr, SymbolEqualityComparer.Default))
                    {
                        return new ContextCore(context.Node, symbol);
                    }
                }
            }

            return null;
        }
        private static void Execute(Compilation compilation, ImmutableArray<ContextCore> declarations, SourceProductionContext context)
        {
            Compilation = compilation;
            Context = context;

            var systemDirective = SF.UsingDirective(SF.ParseName("System"));
            var systemCollectionsGenericDirective = SF.UsingDirective(SF.ParseName("System.Collections.Generic"));
            var systemBuffersBinaryDirective = SF.UsingDirective(SF.ParseName("System.Buffers.Binary"));
            var bsonReaderDirective = SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Reader"));
            var bsonSerializerDirective = SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Serialization"));
            var bsonDirective = SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson"));

            for (int index = 0; index < declarations.Length; index++)
            {
                var decl = declarations[index];
                context.AddSource(decl.SerializerName.ToString(),
                    SF.CompilationUnit()
                      .AddUsings(
                          systemDirective,
                          systemCollectionsGenericDirective,
                          systemBuffersBinaryDirective,
                          bsonReaderDirective,
                          bsonSerializerDirective,
                          bsonDirective)
                      .AddMembers(GenerateSerializer(decl))
                      .NormalizeWhitespace()
                      .ToString());
            }
        }
    }
}
