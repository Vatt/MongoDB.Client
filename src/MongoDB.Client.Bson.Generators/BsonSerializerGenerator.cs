using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;

namespace MongoDB.Client.Bson.Generators
{
    //object - отдельная ветка генератора 
    [Generator]
    partial class BsonSerializerGenerator : ISourceGenerator
    {
        //private List<ClassDeclMeta> meta = new List<ClassDeclMeta>();
        GeneratorExecutionContext _context;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            //System.Diagnostics.Debugger.Launch();
            if (receiver.Candidates.Count == 0)
            {
                return;
            }
            _context = context;
            var masterContext = new MasterContext(CollectSymbols(), context);
            var units = BsonSyntaxGenerator.Create(masterContext);
            context.AddSource($"{Basics.GlobalSerializationHelperGeneratedString}.cs", SourceText.From(GenerateGlobalHelperStaticClass(masterContext), Encoding.UTF8));


            for (int index = 0; index < units.Length; index++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var newSource = units[index].NormalizeWhitespace().ToString();
                context.AddSource(SerializerGenerator.SerializerName(masterContext.Contexts[index]), SourceText.From(newSource!, Encoding.UTF8));
                System.Diagnostics.Debugger.Break();
            }
            _stopwatch.Stop();
            BsonGeneratorErrorHelper.WriteWarn(context, "Generation elapsed: " + _stopwatch.Elapsed.ToString());
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            _stopwatch.Restart();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private List<INamedTypeSymbol> CollectSymbols()
        {
            List<INamedTypeSymbol> symbols = new List<INamedTypeSymbol>();
            foreach (var tree in _context.Compilation.SyntaxTrees)
            {
                foreach (var node in tree.GetRoot().DescendantNodes())
                {
                    SemanticModel model = _context.Compilation.GetSemanticModel(node.SyntaxTree);
                    INamedTypeSymbol? symbol = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                    if (symbol is null)
                    {
                        continue;
                    }

                    foreach (var attr in symbol.GetAttributes())
                    {
                        if (attr.AttributeClass!.ToString().Equals("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute") ||
                            attr.AttributeClass!.ToString().Equals("MongoDB.Client.Bson.Serialization.Attributes.BsonEnumSerializableAttribute"))
                        {
                            symbols.Add(symbol);
                            break;
                        }
                    }
                }
            }

            return symbols;
        }
    }

}
