using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    //object - отдельная ветка генератора 
    [Generator]
    partial class BsonSerializerGenerator : ISourceGenerator
    {
        private List<ClassDeclMeta> meta = new List<ClassDeclMeta>();
        GeneratorExecutionContext _context;

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            //System.Diagnostics.Debugger.Launch();
            if (receiver.Candidates.Count == 0)
            {
                return;
            }
            _context = context;

            ProcessCandidates(receiver.Candidates);
            context.AddSource($"{Basics.GlobalSerializationHelperGeneratedString}.cs", SourceText.From(GenerateGlobalHelperStaticClass(), Encoding.UTF8));
            foreach (var item in meta)
            {
                if (!ReadsMap.SimpleOperations.ContainsKey(item.ClassSymbol.Name))
                {
                    ReadsMap.SimpleOperations.Add(item.ClassSymbol.Name, new GeneratedSerializerRead(item.ClassSymbol, Basics.TryParseBsonNameIdentifier));
                }

            }
            foreach (var item in meta)
            {
                var source = BsonSyntaxGenerator.Create(item)?.NormalizeWhitespace().ToFullString();

                context.AddSource(Basics.GenerateSerializerName(item.ClassSymbol), SourceText.From(source, Encoding.UTF8));
                System.Diagnostics.Debugger.Break();
            }
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
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
                            if (propertySymbol.DeclaredAccessibility == Accessibility.Public)
                            {
                                decl.MemberDeclarations.Add(new MemberDeclarationMeta(propertySymbol));
                            }
                            break;
                        }

                }

            }
        }

    }

}
