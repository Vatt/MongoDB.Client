using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using System.Collections.Generic;
using System.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    //object - отдельная ветка генератора 
    [Generator]
    partial class BsonSerializerGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            var masterContext = new MasterContext(CollectSymbols(context), context);
            var units = Create(masterContext);
            context.AddSource("GlobalSerializationHelperGenerated.cs", SourceText.From(GenerateGlobalHelperStaticClass(masterContext), Encoding.UTF8));
            for (int index = 0; index < units.Length; index++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var source = units[index].NormalizeWhitespace().ToString();
                context.AddSource(SerializerGenerator.SerializerName(masterContext.Contexts[index]), SourceText.From(source, Encoding.UTF8));
                //System.Diagnostics.Debugger.Break();
            }
        }
        public static CompilationUnitSyntax[] Create(MasterContext context)
        {
            CompilationUnitSyntax[] units = new CompilationUnitSyntax[context.Contexts.Count];
            for (int index = 0; index < units.Length; index++)
            {
                units[index] = SF.CompilationUnit()
                    .AddUsings(
                        SF.UsingDirective(SF.ParseName("System")),
                        SF.UsingDirective(SF.ParseName("System.Collections.Generic")),
                        SF.UsingDirective(SF.ParseName("System.Buffers.Binary")))
                    .AddMembers(
                        SF.NamespaceDeclaration(SF.ParseName("MongoDB.Client.Bson.Serialization.Generated"))
                            .AddMembers(SerializerGenerator.GenerateSerializer(context.Contexts[index]))
                    );
            }
            return units;
        }
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private List<(SyntaxNode, INamedTypeSymbol)> CollectSymbols(GeneratorExecutionContext context)
        {
            List<(SyntaxNode, INamedTypeSymbol)> symbols = new ();
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                foreach (var node in tree.GetRoot().DescendantNodes())
                {
                    SemanticModel model = context.Compilation.GetSemanticModel(node.SyntaxTree);
                    INamedTypeSymbol? symbol = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                    if (symbol is null)
                    {
                        continue;
                    }

                    foreach (var attr in symbol.GetAttributes())
                    {
                        if (attr.AttributeClass!.ToString().Equals("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute"))
                        {
                            symbols.Add((node, symbol));
                            break;
                        }
                    }
                }
            }
            return symbols;
        }
    }

}
