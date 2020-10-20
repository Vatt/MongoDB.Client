using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System;
using System.Linq.Expressions;
using System.Linq;

namespace MongoDB.Client.Bson.Generators
{
    [Generator]
    public class BsonSerializatorGenerator : ISourceGenerator
    {
        private List<ClassMapInfo> meta = new List<ClassMapInfo>();
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            //Debugger.Launch();
            CollectMapData(context, receiver.Candidates);
            foreach(var item in meta)
            {
                var builder = GeneratePrologue(item);
                GenerateWriterMethod(builder);
                GenerateReaderMethod(item, builder);
                builder.Append("}}");
                context.AddSource($"{item.ClassName}GeneratedSerializator.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            }

            //Debugger.Launch();

        }
        private StringBuilder GeneratePrologue(ClassMapInfo info)
        {
            var builder = new StringBuilder($@"          
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using System;
namespace MongoDB.Client.Test.Generated
{{      
        public class {info.ClassName}GeneratedSerializator : IBsonSerializable 
        {{
            {GenerateStaticReadOnlyBsonFieldsSpans(info)}
            {GenerateStaticReadOnlyGenericListTypes(info)}
            {GenerateStaticReadOnlyDocumentTypes(info)}
            public {info.ClassName}GeneratedSerializator(){{}}

");
            /*            static {info.ClassName}GeneratedSerializator()
                        {{
                                GlobalSerialization.Serializators[Type.GetType(""MongoDB.Client.Test.Generated.{info.ClassName}GeneratedSerializator"")] = new {info.ClassName}GeneratedSerializator();
                        }} */
                        return builder;
                    }
                    private void GenerateReaderMethod(ClassMapInfo info, StringBuilder builder)
                    {
                        builder.Append($@"
            bool IBsonSerializable.TryParse(ref MongoDBBsonReader reader, out object message)
            {{
                message = default;
                var result = new {info.ClassName}();
                if (!reader.TryGetInt32(out var docLength)) {{ return false; }}
                var unreaded = reader.Remaining + sizeof(int);
                while (unreaded - reader.Remaining < docLength - 1)
                {{
                    if (!reader.TryGetByte(out var type)) {{ return false; }}
                    if (!reader.TryGetCStringAsSpan(out var name)) {{ return false; }}
                    switch (type)
                    {{
                        {GenerateClassFieldMapFieldAssignIfHave(1,  info)}
                        {GenerateClassFieldMapFieldAssignIfHave(2,  info)}
                        {GenerateClassFieldMapFieldAssignIfHave(3,  info)}
                        {/*GenerateClassFieldMapFieldAssignIfHave(4, info)*/
            String.Empty}
            {GenerateClassFieldMapFieldAssignIfHave(54, info)}
            {GenerateClassFieldMapFieldAssignIfHave(7,  info)}
            {GenerateClassFieldMapFieldAssignIfHave(8,  info)}
            {GenerateClassFieldMapFieldAssignIfHave(9,  info)}
            {GenerateClassFieldMapFieldAssignIfHave(10, info)}
            {GenerateClassFieldMapFieldAssignIfHave(16, info)}
            {GenerateClassFieldMapFieldAssignIfHave(18, info)}

            default:
            {{
                throw new ArgumentException(""{info.ClassName}.TryParse  with type {{type}}"");
            }}      
        }}
    }}
    message = result;
    return true;
}}");
        }             
        private string GenerateClassFieldMapFieldAssignIfHave(int type, ClassMapInfo info)
        {
            int TYPE = 0;
            if (type == 54) { TYPE = 5; } else { TYPE = type; }
            StringBuilder builder = new StringBuilder();
            
            if ( !info.MapedFields.Any( (info)=>info.TypeId == type) )
            {
                return String.Empty;
            }
            builder.Append($@"
                case {TYPE}:{{
                        {GenerateCaseReaderReadFromType(type)}
");
            foreach(var fieldInfo in info.MapedFields)
            {
                if (fieldInfo.TypeId != type) { continue; }
                if (fieldInfo.TypeId == 10)
                {
                    builder.Append($@"result.{fieldInfo.ClassField} = null;" );
                    continue;
                }
                if (fieldInfo.TypeId == 3)
                {
                    builder.Append(GenerateReadOtherDocument(type, info, fieldInfo));
                    continue;
                }
                if (fieldInfo.TypeId == type)
                {
                    if (fieldInfo.BsonFieldAlias == null)
                    {
                        builder.Append($@"
                        if ( name.SequenceEqual({info.ClassName}{fieldInfo.BsonField}) )
                        {{
                            result.{fieldInfo.ClassField} = value;
                        }}
");
                    }
                    else
                    {
                            builder.Append($@"
                        if ( name.SequenceEqual({info.ClassName}{fieldInfo.BsonFieldAlias}) )
                        {{
                            result.{fieldInfo.ClassField} = value;
                        }}
");
                    }

                }

            }
            builder.Append("\t\t\t\t\tbreak;\n\t\t\t\t}");
            return builder.ToString();
        }
        private string GenerateReadOtherDocument(int type,ClassMapInfo classinfo, MapFieldInfo info)
        {
            
            StringBuilder builder = new StringBuilder();
            if (info.BsonFieldAlias == null)
            {
                builder.Append($@"
                            if ( name.SequenceEqual({classinfo.ClassName}{info.BsonField}) )");
            }
            else
            {
                builder.Append($@"
                            if ( name.SequenceEqual({classinfo.ClassName}{info.BsonFieldAlias}))");
            }
            builder.Append($@"
                            {{
                               if ( !GlobalSerialization.Serializators.TryGetValue({classinfo.ClassName}{info.TypeAlias}TypeDocument{info.Id}, out var serializator))
                               {{
                                    throw new Exception(""{classinfo.ClassName}.{info.ClassField} not a IBsonSerialize"");
                               }}
                               if ( !serializator.TryParse(ref reader, out var value)){{ return false;}}
                               result.{info.ClassField} = value as {info.Type};
                            }}

            ");
            return builder.ToString();
        }
        private string GenerateCaseReaderReadFromType(int type)
        {
            switch (type)
            {
                case 1:
                    {
                        return "if (!reader.TryGetDouble(out var value)) { return false; }";
                    }
                case 2:
                    {
                        return "if (!reader.TryGetString(out var value)) { return false; }";

                    }
                case 3:
                    {
                        //return "if (!reader.TryParseDocument(parent, out var value)) { return false; }";
                        return String.Empty;

                    }
                case 4:
                    {
                        return "if (!reader.TryGetArray(out var value)) { return false; }";

                    }
                case 54:
                    {
                        return "if (!reader.TryGetBinaryDataGuid(out var value)) { return false; }";
                    }
                case 7:
                    {
                        return "if (!reader.TryGetObjectId(out var value)) { return false; }";
                    }
                case 8:
                    {
                        return "if (!reader.TryGetBoolean(out var value)) { return false; }";
                    }
                case 9:
                    {
                        return "if (!reader.TryGetUTCDatetime(out var value)) { return false; }";
                    }
                case 10:
                    {
                        //TODO:FIX IT
                        //element = BsonElement.Create(parent, name);
                        //return true;
                        //throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(GenerateCaseReaderReadFromType)}  with type {type}");
                        return "";
                    }
                case 16:
                    {
                        return "if (!reader.TryGetInt32(out var value)) { return false; }";
                    }
                case 18:
                    {
                        return "if (!reader.TryGetInt64(out var value)) { return false; }";
                    }
                default:
                    {
                        throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(GenerateCaseReaderReadFromType)}  with type {type}");
                    }
            }
        }

        public void GenerateWriterMethod(StringBuilder builder)
        {
            builder.Append("\n void IBsonSerializable.Write(object message)\n{ \nthrow new NotImplementedException(); \n}\n");
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private void CollectMapData(GeneratorExecutionContext context, List<ClassDeclarationSyntax> candidates)
        {
            
            INamedTypeSymbol attrName = context.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Serialization.Attributes.BsonElementField");
            foreach(var candidate in candidates)
            {
                var info = new ClassMapInfo();
                info.ClassName = candidate.Identifier.Text;
                foreach (var member in candidate.Members)
                {
                    
                    if (member is FieldDeclarationSyntax fieldDecl && fieldDecl.AttributeLists.Count > 0)
                    {
                        SemanticModel model = context.Compilation.GetSemanticModel(fieldDecl.SyntaxTree);
                        
                        foreach (var variable in fieldDecl.Declaration.Variables)
                        {
                            
                            IFieldSymbol fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
                            foreach (var attr in fieldSymbol.GetAttributes())
                            {
                                
                                if (attr.AttributeClass.Equals(attrName, SymbolEqualityComparer.Default))
                                {                                    
                                    var mapFieldInfo = new MapFieldInfo();
                                    mapFieldInfo.ClassField = variable.Identifier.ValueText;
                                    if (attr.NamedArguments.Length > 0)
                                    {
                                        foreach(var namedAttrArg in attr.NamedArguments)
                                        {
                                            if (namedAttrArg.Key.Equals("ElementName"))
                                            {
                                                mapFieldInfo.BsonField = (string)namedAttrArg.Value.Value;
                                                if (mapFieldInfo.BsonField.Contains(' ') || mapFieldInfo.BsonField.Contains('(') || mapFieldInfo.BsonField.Contains(')'))
                                                {
                                                    mapFieldInfo.BsonFieldAlias = mapFieldInfo.BsonField;
                                                    mapFieldInfo.BsonFieldAlias = mapFieldInfo.BsonFieldAlias.Replace(' ', '_');
                                                    mapFieldInfo.BsonFieldAlias = mapFieldInfo.BsonFieldAlias.Replace('(', '_');
                                                    mapFieldInfo.BsonFieldAlias = mapFieldInfo.BsonFieldAlias.Replace(')', '_');
                                                }
                                            }
                                        }
                                        

                                    }
                                    else
                                    {
                                        mapFieldInfo.BsonField = mapFieldInfo.ClassField;
                                    }        
                                    MatchClassTypeId(fieldSymbol.Type as INamedTypeSymbol, mapFieldInfo);                                    
                                    info.MapedFields.Add(mapFieldInfo);                                    
                                }                                
                            }
                            
                        }                        
                    }                    
                }
                meta.Add(info);
            }
        }
        private void MatchClassTypeId(INamedTypeSymbol symbol, MapFieldInfo info)
        {
            switch (symbol.ToString())
            {
                case "double": 
                    { 
                        info.TypeId = 1; 
                        break; 
                    }
                case "string": 
                    { 
                        info.TypeId = 2; 
                        break; 
                    }                       
                case "System.Guid":
                    {
                        info.TypeId = 54; 
                        break;
                    }
                case "MongoDB.Client.Bson.Document.BsonObjectId":
                    {
                        info.TypeId = 7;
                        break; 
                    }
                case "bool":
                    {
                        info.TypeId = 8;
                        break;
                    }
                case "System.DateTimeOffset":
                    {
                        info.TypeId = 9;
                        info.Type = "System.DateTimeOffset";
                        break;
                    }
                case "object":
                    {
                        info.TypeId = 10; //BSON null
                        break;
                    }
                case "int":
                    {
                        info.TypeId = 16;
                        break;
                    }
                case "long":
                    {
                        info.TypeId = 18;
                        break;
                    }
                default:
                    {
                        if (isList(symbol.ToString())) 
                        {
                            info.TypeId = 4;
                            if (symbol.TypeArguments.Length > 1)
                            {
                                throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(MatchClassTypeId)} in array type generics count > 1");
                            }
                            info.GenericType =  symbol.TypeArguments[0].ToString();
                            info.GenericTypeAlias = info.GenericType.Replace('.', '_');
                            return;
                            
                        }
                        info.isDocument = true;
                        info.TypeId = 3;
                        info.Type = symbol.ToString();
                        info.TypeAlias = info.Type.Replace('.', '_');
                        return;
                       // throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(MatchClassTypeId)} with type {symbol.ToString()}");
                    }                
            }            
            bool isList(string stringSymbol)
            {
                return stringSymbol.Contains("System.Collections.Generic.List") || stringSymbol.Contains("System.Collections.Generic.IList");
            }

        }
        private string GenerateStaticReadOnlyBsonFieldsSpans(ClassMapInfo info)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var fieldInfo in info.MapedFields)
            {
                var len = Encoding.UTF8.GetByteCount(fieldInfo.BsonField);
                var bytes = Encoding.UTF8.GetBytes(fieldInfo.BsonField);
                if (fieldInfo.BsonFieldAlias != null)
                {
                    builder.Append($"\n\t    private static ReadOnlySpan<byte> {info.ClassName}{fieldInfo.BsonFieldAlias} => new byte[{len}] {{");
                }
                else 
                {
                    builder.Append($"\n\t    private static ReadOnlySpan<byte> {info.ClassName}{fieldInfo.BsonField} => new byte[{len}] {{");
                }
                
                
                for (var ind = 0; ind < len; ind++ )
                {
                    if (ind == len -1 )
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
        private string GenerateStaticReadOnlyGenericListTypes(ClassMapInfo info)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var fieldInfo in info.MapedFields)
            {
                if(fieldInfo.GenericType != null)
                {
                    builder.Append($@"
            private static readonly Type {info.ClassName}{fieldInfo.GenericTypeAlias}GenericArgument = Type.GetType(""{fieldInfo.GenericType}"");");
                }
            }
            return builder.ToString();
        }
        private string GenerateStaticReadOnlyDocumentTypes(ClassMapInfo info)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var fieldInfo in info.MapedFields)
            {
                if (fieldInfo.Type != null)
                {
                    builder.Append($@"
            private static readonly Type {info.ClassName}{fieldInfo.TypeAlias}TypeDocument{fieldInfo.Id} = Type.GetType(""{fieldInfo.Type}"");");
                }
            }
            return builder.ToString();
        }
    }
    
}
