using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;
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

#if DEBUG
            var all = System.Diagnostics.Stopwatch.StartNew();
#endif


            Context = context;
            Compilation = Context.Compilation;

            var candidates = (context.SyntaxReceiver as BsonAttributeReciver)!.Candidates;


            var symbols = CollectSymbols(Compilation, candidates, context.CancellationToken);
            var masterContext = new MasterContext(symbols, context.CancellationToken);
            var units = Create(masterContext, context.CancellationToken);
            for (int index = 0; index < units.Length; index++)
            {
                var source = units[index].NormalizeWhitespace().ToString();
                context.AddSource(masterContext.Contexts[index].SerializerName.ToString(), SourceText.From(source, Encoding.UTF8));
            }

#if DEBUG
            SyntaxGenerator.Diagnostics.GeneratorDiagnostics.ReportDuration("All", all.Elapsed);
#endif
        }


        public static CompilationUnitSyntax[] Create(MasterContext master, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax[] units = new CompilationUnitSyntax[master.Contexts.Count];

            var systemDirective = SF.UsingDirective(SF.ParseName("System"));
            var systemCollectionsGenericDirective = SF.UsingDirective(SF.ParseName("System.Collections.Generic"));
            var systemBuffersBinaryDirective = SF.UsingDirective(SF.ParseName("System.Buffers.Binary"));
            var bsonReaderDirective = SF.UsingDirective(SF.ParseName("MongoDB.Client.Bson.Reader"));

            for (int index = 0; index < units.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                units[index] = SF.CompilationUnit()
                    .AddUsings(
                        systemDirective,
                        systemCollectionsGenericDirective,
                        systemBuffersBinaryDirective,
                        bsonReaderDirective)
                    .AddMembers(SerializerGenerator.GenerateSerializer(master.Contexts[index]));
            }
            return units;
        }


        private HashSet<BsonSerializerNode> CollectSymbols(Compilation compilation, HashSet<SyntaxNode> candidates, CancellationToken token)
        {
            HashSet<BsonSerializerNode> symbols = new();
            var bsonAttribute = SerializerGenerator.BsonSerializableAttr;

            foreach (var node in candidates)
            {
                token.ThrowIfCancellationRequested();
                SemanticModel model = compilation.GetSemanticModel(node.SyntaxTree);

                if (model.GetDeclaredSymbol(node) is INamedTypeSymbol symbol)
                {
                    foreach (var attr in symbol.GetAttributes())
                    {
                        if (attr.AttributeClass.Equals(bsonAttribute, SymbolEqualityComparer.Default))
                        {
                            symbols.Add(new BsonSerializerNode(node, symbol));
                            break;
                        }
                    }
                }
            }
            return symbols;
        }

        public class BsonAttributeReciver : ISyntaxReceiver
        {
            public HashSet<SyntaxNode> Candidates { get; } = new HashSet<SyntaxNode>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Count > 0)
                {
                    Candidates.Add(syntaxNode);
                    return;
                }
                if (syntaxNode is StructDeclarationSyntax structDecl && structDecl.AttributeLists.Count > 0)
                {
                    Candidates.Add(syntaxNode);
                    return;
                }
                if (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.AttributeLists.Count > 0)
                {
                    Candidates.Add(syntaxNode);
                    return;
                }
            }
        }

        internal readonly struct BsonSerializerNode : IEquatable<BsonSerializerNode>
        {
            public BsonSerializerNode(SyntaxNode syntaxNode, INamedTypeSymbol typeSymbol)
            {
                SyntaxNode = syntaxNode;
                TypeSymbol = typeSymbol;
            }

            public SyntaxNode SyntaxNode { get; }
            public INamedTypeSymbol TypeSymbol { get; }

            public void Deconstruct(out SyntaxNode syntaxNode, out INamedTypeSymbol typeSymbol)
            {
                syntaxNode = SyntaxNode;
                typeSymbol = TypeSymbol;
            }

            public override bool Equals(object obj)
            {
                return obj is BsonSerializerNode candidate && Equals(candidate);
            }

            public bool Equals(BsonSerializerNode other)
            {
                return EqualityComparer<SyntaxNode>.Default.Equals(SyntaxNode, other.SyntaxNode) &&
                       EqualityComparer<INamedTypeSymbol>.Default.Equals(TypeSymbol, other.TypeSymbol);
            }

            public override int GetHashCode()
            {
                int hashCode = 383889175;
                hashCode = hashCode * -1521134295 + EqualityComparer<SyntaxNode>.Default.GetHashCode(SyntaxNode);
                hashCode = hashCode * -1521134295 + EqualityComparer<INamedTypeSymbol>.Default.GetHashCode(TypeSymbol);
                return hashCode;
            }
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BsonAttributeReciver());
        }
    }
}
