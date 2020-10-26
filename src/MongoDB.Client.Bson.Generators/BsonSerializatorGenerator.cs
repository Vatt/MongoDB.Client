using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    public static class GlobalSerializationHelperGenerated{{
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
                    //builder.Append($"\n\t\tpublic static readonly  {info.ClassSymbol.Name}GeneratedSerializer {info.ClassSymbol.Name}GeneratedSerializerStaticField = new {info.ClassSymbol.Name}GeneratedSerializer();");
                    builder.Append($"\n\t\tpublic static readonly  IGenericBsonSerializer<{info.ClassSymbol.Name}>  {info.ClassSymbol.Name}GeneratedSerializerStaticField = new {info.ClassSymbol.Name}GeneratedSerializer();");
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
                foreach(var decl in meta)
                {

                    builder.Append($@"
                    pairs[{index}] = KeyValuePair.Create<Type, IBsonSerializer>(typeof({decl.ClassSymbol.Name}), {decl.ClassSymbol.Name}GeneratedSerializerStaticField);
                    ");
                    index ++;
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
            AddCandidatesToOperationsMap();
            foreach (var item in meta)
            {
                var gen = BsonSyntaxGenerator.Create(item);
                //gen.DeclarTryParseMethod();
                gen.Build();
                
                //var builder = Generate(item);
                //context.AddSource($"{item.ClassSymbol.Name}GeneratedSerializator.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            }

            //Debugger.Launch();

        }
        private StringBuilder Generate(ClassDeclMeta info)
        {
            var builder = new StringBuilder($@"          
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
using {info.StringNamespace};
namespace MongoDB.Client.Bson.Serialization.Generated
{{      
        public class {info.ClassSymbol.Name}GeneratedSerializer : IGenericBsonSerializer<{info.ClassSymbol.Name}> 
        {{
            {GenerateStaticReadOnlyBsonFieldsSpans(info)}
            public {info.ClassSymbol.Name}GeneratedSerializer(){{}}
            void IBsonSerializer.Write(object message){{ throw new NotImplementedException(); }}
            {GenerateReaderMethod(info)}
         }}
        
}}");
            return builder;
        }

        public void AddCandidatesToOperationsMap()
        {
            foreach (var decl in meta)
            {
                if (!BsonGeneratorReadOperations.GeneratedSerializatorsOperations.ContainsKey(decl.ClassSymbol.Name))
                {
                    BsonGeneratorReadOperations.GeneratedSerializatorsOperations.Add(
                    decl.ClassSymbol.Name,
                    @$"if ( !GlobalSerializationHelperGenerated.{decl.ClassSymbol.Name}GeneratedSerializerStaticField.TryParse(ref reader, out {{0}})){{{{ return false;}}}}"
                    );
                }

            }
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
        private string GenerateStaticReadOnlyBsonFieldsSpans(ClassDeclMeta info)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var fieldinfo in info.MemberDeclarations)
            {
                int len = 0;
                byte[] bytes;
                var haveNamedElement = fieldinfo.TryGetElementNameFromBsonAttribute(out var bsonField);
                if (!haveNamedElement)
                {
                    len = Encoding.UTF8.GetByteCount(fieldinfo.DeclSymbol.Name);
                    bytes = Encoding.UTF8.GetBytes(fieldinfo.DeclSymbol.Name);
                }
                else
                {
                    len = Encoding.UTF8.GetByteCount(bsonField);
                    bytes = Encoding.UTF8.GetBytes(bsonField);
                }

                builder.Append($"\n\t\t    private static ReadOnlySpan<byte> {info.ClassSymbol.Name}{fieldinfo.StringFieldNameAlias} => new byte[{len}] {{");


                for (var ind = 0; ind < len; ind++)
                {
                    if (ind == len - 1)
                    {
                        builder.Append($"{bytes[ind]}");
                    }
                    else
                    {
                        builder.Append($"{bytes[ind]}, ");
                    }

                }
                builder.Append("};");
            }
            return builder.ToString();
        }
    }

}
