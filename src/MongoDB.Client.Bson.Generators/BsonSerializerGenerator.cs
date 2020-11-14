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

namespace MongoDB.Client.Bson.Generators
{
    //object - отдельная ветка генератора 
    [Generator]
    partial class BsonSerializerGenerator : ISourceGenerator
    {
        private List<ClassDeclMeta> meta = new List<ClassDeclMeta>();
        GeneratorExecutionContext _context;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            System.Diagnostics.Debugger.Launch();
            if (receiver.Candidates.Count == 0)
            {
                return;
            }
            _context = context;

            ProcessCandidates(receiver.Candidates);
            ProcessEnums(receiver.Enums);
            var masterContext = new MasterContext(CollectSymbols(), context);
            OperationBase.meta = meta;
            var units = BsonSyntaxGenerator.Create(masterContext);
            context.AddSource($"{Basics.GlobalSerializationHelperGeneratedString}.cs", SourceText.From(GenerateGlobalHelperStaticClass(), Encoding.UTF8));

            foreach (var item in meta)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                if (!TypeMap.BaseOperations.ContainsKey(item.ClassSymbol.Name))
                {
                    TypeMap.BaseOperations.Add(item.ClassSymbol.Name, new GeneratedSerializerRW(item.ClassSymbol));
                }
            }
            for (int index = 0; index < meta.Count; index++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var newSource = units[index].NormalizeWhitespace().ToString();
                var source = BsonSyntaxGenerator.Create(meta[index])?.NormalizeWhitespace().ToFullString();
                context.AddSource(Basics.GenerateSerializerName(meta[index].ClassSymbol), SourceText.From(source!, Encoding.UTF8));
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
                        if (attr.AttributeClass!.ToString().Equals("MongoDB.Client.Bson.Serialization.Attributes.BsonSerializableAttribute"))
                        {
                            symbols.Add(symbol);
                            break;
                        }
                    }
                }
            }

            return symbols;
        }
        private void ProcessEnums(List<EnumDeclarationSyntax> enums)
        {
            foreach (var item in  enums)
            {

                SemanticModel classModel = _context.Compilation.GetSemanticModel(item.SyntaxTree);
                var symbol = classModel.GetDeclaredSymbol(item);
                var declmeta = new ClassDeclMeta(symbol);
                foreach (var member in item.Members)
                {
                    SemanticModel memberModel = _context.Compilation.GetSemanticModel(member.SyntaxTree);
                    ISymbol enumMember = memberModel.GetDeclaredSymbol(member);
                    if (IsIgnore(enumMember))
                    {
                        continue;
                    }
                    if (enumMember.DeclaredAccessibility == Accessibility.Public)
                    {
                        declmeta.MemberDeclarations.Add(new MemberDeclarationMeta(enumMember));
                    }
                }
                if (declmeta.MemberDeclarations.Count > 0)
                {
                    meta.Add(declmeta);
                }
            }
        }
        private void ProcessCandidates(List<TypeDeclarationSyntax> candidates)
        {
            foreach (var candidate in candidates)
            {

                SemanticModel classModel = _context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                var symbol = classModel.GetDeclaredSymbol(candidate);
                var declmeta = new ClassDeclMeta(symbol);
                switch (candidate)
                {
                    case ClassDeclarationSyntax classdecl:
                        {
                            AddMemberIfNeed(declmeta, classdecl);
                            break;
                        }
                    case StructDeclarationSyntax structdecl:
                        {
                            AddMemberIfNeed(declmeta, structdecl);
                            break;
                        }
                        //case RecordDeclarationSyntax recorddecl:
                        //    {
                        //        AddMemberIfNeed(declmeta, recorddecl);
                        //        break;
                        //    }
                }
                if (declmeta.MemberDeclarations.Count > 0)
                {
                    meta.Add(declmeta);
                }

            }
        }
        private void AddMemberIfNeed(ClassDeclMeta decl, TypeDeclarationSyntax syntax)
        {
            foreach (var member in syntax.Members)
            {
                switch (member)
                {
                    case FieldDeclarationSyntax fielddecl:
                        {
                            SemanticModel memberModel = _context.Compilation.GetSemanticModel(fielddecl.SyntaxTree);

                            foreach (var variable in fielddecl.Declaration.Variables)
                            {

                                IFieldSymbol fieldSymbol = memberModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                                if (IsIgnore(fieldSymbol))
                                {
                                    continue;
                                }

                                if (fieldSymbol.DeclaredAccessibility == Accessibility.Public)
                                {
                                    decl.MemberDeclarations.Add(new MemberDeclarationMeta(fieldSymbol));
                                }
                            }
                            break;
                        }
                    case PropertyDeclarationSyntax propdecl:
                        {
                            SemanticModel memberModel = _context.Compilation.GetSemanticModel(propdecl.SyntaxTree);
                            ISymbol propertySymbol = memberModel.GetDeclaredSymbol(propdecl);
                            if (IsIgnore(propertySymbol))
                            {
                                continue;
                            }
                            if (propertySymbol.DeclaredAccessibility == Accessibility.Public)
                            {
                                decl.MemberDeclarations.Add(new MemberDeclarationMeta(propertySymbol));
                            }
                            break;
                        }

                }

            }
        }
        private bool IsIgnore(ISymbol sym)
        {
            foreach (var attr in sym.GetAttributes())
            {
                if (attr.AttributeClass is null)
                {
                    continue;
                }
                if (attr.AttributeClass.Name.Equals("BsonIgnore"))
                {
                    return true;
                }
            }
            return false;
        }

    }

}
