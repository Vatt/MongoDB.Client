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
    partial class BsonSerializatorGenerator : ISourceGenerator
    {
        private List<ClassDeclMeta> meta = new List<ClassDeclMeta>();
        GeneratorExecutionContext _context;
        public string GenerateGlobalHelperStaticClass()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using System;
using System.Collections.Generic;
namespace MongoDB.Client.Bson.Serialization.Generated{{
    public static class {Basics.GlobalSerializationHelperGeneratedString}{{
        {GenerateFields()}
        {GenerateGetSeriazlizersMethod()}
    }}
}}");
            return builder.ToString();
            string GenerateFields()
            {
                var builder = new StringBuilder();
                foreach (var info in meta)
                {
                    builder.Append($"\n\t\tpublic static readonly  IGenericBsonSerializer<{info.ClassSymbol.Name}>  {Basics.GenerateSerializerNameStaticField(info.ClassSymbol)} = new {Basics.GenerateSerializerName(info.ClassSymbol)};");
                }
                return builder.ToString();
            }
            string GenerateGetSeriazlizersMethod()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($@"
                public static KeyValuePair<Type, IBsonSerializer>[]  GetGeneratedSerializers()
                {{
                    var pairs = new KeyValuePair<Type, IBsonSerializer>[{meta.Count}];                    
                ");
                int index = 0;
                foreach (var decl in meta)
                {

                    builder.Append($@"
                    pairs[{index}] = KeyValuePair.Create<Type, IBsonSerializer>(typeof({decl.ClassSymbol.Name}), {Basics.GenerateSerializerNameStaticField(decl.ClassSymbol)});
                    ");
                    index++;
                }
                builder.Append(@"
                    return pairs;
                }
                ");
                return builder.ToString();
            }
        }
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            System.Diagnostics.Debugger.Launch();
            if (receiver.Candidates.Count == 0)
            {
                return;
            }
            _context = context;

            CollectMapData(context, receiver.Candidates);
            context.AddSource($"GlobalSerializationHelperGenerated.cs", SourceText.From(GenerateGlobalHelperStaticClass(), Encoding.UTF8));
            foreach (var item in meta)
            {
                if (!ReadsMap.SimpleOperations.ContainsKey(item.ClassSymbol.Name))
                {
                    ReadsMap.SimpleOperations.Add(item.ClassSymbol.Name, new GeneratedSerializerRead(item.ClassSymbol, Basics.TryParseBsonNameIdentifier));
                }
                
            }
            foreach (var item in meta)
            {
                var source = BsonSyntaxGenerator.Create(item).NormalizeWhitespace().ToFullString();

                context.AddSource(Basics.GenerateSerializerName(item.ClassSymbol), SourceText.From(source, Encoding.UTF8));
            }

            //Debugger.Launch();

        }
        public void Initialize(GeneratorInitializationContext context)
        {

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private void CollectMapData(GeneratorExecutionContext context, List<TypeDeclarationSyntax> candidates)
        {

            INamedTypeSymbol attrName = context.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonElementField");
            foreach (var candidate in candidates)
            {

                SemanticModel classModel = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                var symbol = classModel.GetDeclaredSymbol(candidate);
                var info = new ClassDeclMeta(symbol);
                foreach (var member in candidate.Members)
                {

                    if (member is FieldDeclarationSyntax fieldDecl)
                    {
                        SemanticModel memberModel = context.Compilation.GetSemanticModel(fieldDecl.SyntaxTree);

                        foreach (var variable in fieldDecl.Declaration.Variables)
                        {

                            IFieldSymbol fieldSymbol = memberModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                            if (fieldSymbol.DeclaredAccessibility == Accessibility.Public)
                            {
                                info.MemberDeclarations.Add(new MemberDeclarationMeta(fieldSymbol));
                            }
                        }
                    }
                    if (member is PropertyDeclarationSyntax propDecl)
                    {
                        SemanticModel memberModel = context.Compilation.GetSemanticModel(propDecl.SyntaxTree);

                        ISymbol propertySymbol = memberModel.GetDeclaredSymbol(propDecl);
                        if (propertySymbol.DeclaredAccessibility == Accessibility.Public)
                        {
                            info.MemberDeclarations.Add(new MemberDeclarationMeta(propertySymbol));
                        }
                    }
                }
                meta.Add(info);
            }
        }

    }

}
