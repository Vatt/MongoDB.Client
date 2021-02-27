using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
using System;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    //object - отдельная ветка генератора 
    [Generator]
    class BsonSerializerGenerator : ISourceGenerator
    {
        public static GeneratorExecutionContext Context;
        public static Compilation Compilation;
        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            // var all = Stopwatch.StartNew();
            Context = context;
            Compilation = Context.Compilation;
            //TypeLib.Init(Compilation);
            //GeneratorDiagnostics.Init(context);

            try
            {
                var symbols = CollectSymbols(context);
                var masterContext = new MasterContext(symbols, context.CancellationToken);
                var units = Create(masterContext, context.CancellationToken);
                for (int index = 0; index < units.Length; index++)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    var source = units[index].NormalizeWhitespace().ToString();
                    //context.AddSource(SerializerGenerator.SerializerName(masterContext.Contexts[index]), SourceText.From(source, Encoding.UTF8));
                    context.AddSource(masterContext.Contexts[index].SerializerName.ToString(), SourceText.From(source, Encoding.UTF8));
                }
            }
            catch (OperationCanceledException)
            {
                // nothing
            }
            catch (Exception ex)
            {
                GeneratorDiagnostics.ReportUnhandledException(ex);
            }

            //GeneratorDiagnostics.ReportDuration("All", all.Elapsed);
        }


        public static CompilationUnitSyntax[] Create(MasterContext master, System.Threading.CancellationToken cancellationToken)
        {
            CompilationUnitSyntax[] units = new CompilationUnitSyntax[master.Contexts.Count];

            var systemDirective = SF.UsingDirective(SF.ParseName("System"));
            var systemCollectionsGenericDirective = SF.UsingDirective(SF.ParseName("System.Collections.Generic"));
            var systemBuffersBinaryDirective = SF.UsingDirective(SF.ParseName("System.Buffers.Binary"));

            for (int index = 0; index < units.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                units[index] = SF.CompilationUnit()
                    .AddUsings(
                        systemDirective,
                        systemCollectionsGenericDirective,
                        systemBuffersBinaryDirective)
                    .AddMembers(SerializerGenerator.GenerateSerializer(master.Contexts[index]));
            }
            return units;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private List<(SyntaxNode, INamedTypeSymbol)> CollectSymbols(GeneratorExecutionContext context)
        {
            List<(SyntaxNode, INamedTypeSymbol)> symbols = new();
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(tree);

                foreach (var node in tree.GetRoot().DescendantNodesAndSelf())
                {
                    if (model.GetDeclaredSymbol(node) is INamedTypeSymbol symbol)
                    {
                        foreach (var attr in symbol.GetAttributes())
                        {
                            if (attr.AttributeClass.Equals(SerializerGenerator.BsonSerializableAttr, SymbolEqualityComparer.Default))
                            {
                                symbols.Add((node, symbol));
                                break;
                            }
                        }
                    }
                }
            }
            return symbols;
        }
    }

}
