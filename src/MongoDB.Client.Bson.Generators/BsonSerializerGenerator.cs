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
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
            var all = System.Diagnostics.Stopwatch.StartNew();
#endif

            Context = context;
            Compilation = Context.Compilation;

            var symbols = (context.SyntaxContextReceiver as SemanticBsonAttributeReciver)!.Symbols;

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

        public class SemanticBsonAttributeReciver : ISyntaxContextReceiver
        {
            public HashSet<BsonSerializerNode> Symbols { get; } = new HashSet<BsonSerializerNode>();

            INamedTypeSymbol BsonSerializableAttr = null;

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                var model = context.SemanticModel;
                BsonSerializableAttr ??= model.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute")!;

                switch (context.Node)
                {
                    case ClassDeclarationSyntax classDecl when classDecl.AttributeLists.Count > 0:
                    case StructDeclarationSyntax structDecl when structDecl.AttributeLists.Count > 0:
                    case RecordDeclarationSyntax recordDecl when recordDecl.AttributeLists.Count > 0:
                        if (model.GetDeclaredSymbol(context.Node) is INamedTypeSymbol symbol)
                        {
                            foreach (var attr in symbol.GetAttributes())
                            {
                                if (attr.AttributeClass.Equals(BsonSerializableAttr, SymbolEqualityComparer.Default))
                                {
                                    Symbols.Add(new BsonSerializerNode(context.Node, symbol));
                                    break;
                                }
                            }
                        }
                        break;

                    default:
                        break;

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
            context.RegisterForSyntaxNotifications(() => new SemanticBsonAttributeReciver());
        }
    }
}
