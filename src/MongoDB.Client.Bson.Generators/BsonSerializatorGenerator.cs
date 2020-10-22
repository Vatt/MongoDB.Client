using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;


namespace MongoDB.Client.Bson.Generators
{
    [Generator]
    public class BsonSerializatorGenerator : ISourceGenerator
    {
        private List<ClassDecl> meta = new List<ClassDecl>();
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
    }}
}}");
            return builder.ToString();
            string GenerateFields()
            {
                var builder = new StringBuilder();
                foreach (var info in meta)
                {
                    builder.Append($"\n\t\tpublic static readonly  IBsonSerializable {info.ClassSymbol.Name}GeneratedSerializatorStaticField = new {info.ClassSymbol.Name}GeneratedSerializator();");
                }
                return builder.ToString();
            }
        }
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) { return; }
            //System.Diagnostics.Debugger.Launch();
            if (receiver.Candidates.Count == 0)
            {
                return;
            }
            CollectMapData(context, receiver.Candidates);
            context.AddSource($"GlobalSerializationHelperGenerated.cs", SourceText.From(GenerateGlobalHelperStaticClass(), Encoding.UTF8));
            foreach (var item in meta)
            {
                var builder = Generate(item);
                context.AddSource($"{item.ClassSymbol.Name}GeneratedSerializator.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            }

            //Debugger.Launch();

        }
        private StringBuilder Generate(ClassDecl info)
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
        public class {info.ClassSymbol.Name}GeneratedSerializator : IBsonSerializable 
        {{
            {GenerateStaticReadOnlyBsonFieldsSpans(info)}
            public {info.ClassSymbol.Name}GeneratedSerializator(){{}}
            void IBsonSerializable.Write(object message){{ throw new NotImplementedException(); }}
            {GenerateReaderMethod(info)}
         }}
        
}}");
            return builder;
        }
        private string GenerateReaderMethod(ClassDecl info)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
            bool IBsonSerializable.TryParse(ref MongoDBBsonReader reader, out object message)
            {{
                message = default;
                var result = new {info.ClassSymbol.Name}();
                if (!reader.TryGetInt32(out var docLength)) {{ return false; }}
                var unreaded = reader.Remaining + sizeof(int);
                while (unreaded - reader.Remaining < docLength - 1)
                {{
                    if (!reader.TryGetByte(out var bsonType)) {{ return false; }}
                    if (!reader.TryGetCStringAsSpan(out var bsonName)) {{ return false; }}                                               
                    {GenerateReads(info)}

                    throw new ArgumentException($""{info.ClassSymbol.Name}.TryParse  with bson type number {{bsonType}}"");      
                }}
                if ( !reader.TryGetByte(out var endMarker)){{ return false; }}
                if (endMarker != '\x00')
                {{
                    throw new ArgumentException(""{info.ClassSymbol.Name}GeneratedSerializator.TryParse End document marker missmatch"");
                }}

                message = result;
                return true;
            }}

        ");
            return builder.ToString();
        }
        private string GenerateReads(ClassDecl info)
        {
            StringBuilder buidler = new StringBuilder();
            foreach (var declinfo in info.MemberDeclarations)
            {
                buidler.Append($@"
                    if (bsonName.SequenceEqual( {info.ClassSymbol.Name}{declinfo.StringFieldNameAlias}))
                    {{
                        if( bsonType == 10 )
                        {{
                            result.{declinfo.DeclSymbol.Name} = default;
                            continue;
                        }}
                        {GenerateReadsAndAssign(info, declinfo)}
                        continue;
                    }}
                ");
            }
            return buidler.ToString();
        }

        private string GenerateReadsAndAssign(ClassDecl info, MemberDeclarationInfo declinfo)
        {
            StringBuilder builder = new StringBuilder();
            switch (declinfo.DeclType.Name)
            {
                case "Double":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetDouble(out var value)) {{ return false; }}                           
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetDouble(out var result.{declinfo.DeclSymbol.Name})) {{ return false; }}                           
                        ");
                        }

                        break;
                    }
                case "String":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetString(out var value)) {{ return false; }}                            
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetString(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                            
                        ");
                        }

                        break;
                    }
                case "BsonDocument":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryParseDocument(null, out var value)) {{ return false; }}                        
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryParseDocument(null, out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                            
                        ");
                        }
                        break;
                    }
                case "4": //Массив
                    {
                        //return "if (!reader.TryParseDocument(parent, out var value)) { return false; }";
                        return String.Empty;

                    }

                case "Guid":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if( bsonType == 5 )
                        {{
                            if (!reader.TryGetBinaryDataGuid(out var value)) {{ return false; }}
                            result.{declinfo.DeclSymbol.Name} = value;
                            continue;
                        }}
                        if( bsonType == 2 )
                        {{
                            if (!reader.TryGetGuidFromString(out var value)) {{ return false; }}
                            result.{declinfo.DeclSymbol.Name} = value;
                            continue;
                        }}
                        throw new ArgumentException(""{declinfo.DeclSymbol.Name} unsupported Guid type"");
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if( bsonType == 5 )
                        {{
                            if (!reader.TryGetBinaryDataGuid(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}
                            continue;
                        }}
                        if( bsonType == 2 )
                        {{
                            if (!reader.TryGetGuidFromString(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}
                            continue;
                        }}
                        throw new ArgumentException(""{declinfo.DeclSymbol.Name} unsupported Guid type"");
                        ");
                        }

                        break;
                    }
                case "BsonObjectId":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetObjectId(out var value)) {{ return false; }}                      
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetObjectId(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                      
                        ");
                        }

                        break;
                    }
                case "Boolean":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetBoolean(out var value)) {{ return false; }}                            
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetBoolean(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                            
                        ");
                        }

                        break;
                    }
                case "DateTimeOffset":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if( bsonType == 3 )
                        {{
                            if (!reader.TryGetDatetimeFromDocument(out var value)) {{ return false; }}
                            result.{declinfo.DeclSymbol.Name} = value;
                            continue;
                        }}
                        if( bsonType == 9 )
                        {{
                            if (!reader.TryGetUTCDatetime(out var value)) {{ return false; }}
                            result.{declinfo.DeclSymbol.Name} = value;
                            continue;
                        }}
                        if( bsonType == 18 )
                        {{
                            if (!reader.TryGetUTCDatetime(out var value)) {{ return false; }}
                            result.{declinfo.DeclSymbol.Name} = value;
                            continue;
                        }}
                        throw new ArgumentException(""{declinfo.DeclSymbol.Name} unsupported DateTimeOffset type"");
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if( bsonType == 3 )
                        {{
                            if (!reader.TryGetDatetimeFromDocument(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}
                            continue;
                        }}
                        if( bsonType == 9 )
                        {{
                            if (!reader.TryGetUTCDatetime(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}
                            continue;
                        }}
                        if( bsonType == 18 )
                        {{
                            if (!reader.TryGetUTCDatetime(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}
                            continue;
                        }}
                        throw new ArgumentException(""{declinfo.DeclSymbol.Name} unsupported DateTimeOffset type"");
                        ");
                        }

                        break;
                    }
                case "Int32":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetInt32(out var value)) {{ return false; }}    
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetInt32(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                            
                        ");
                        }

                        break;
                    }
                case "Int64":
                    {
                        if (declinfo.IsProperty)
                        {
                            builder.Append($@"
                        if (!reader.TryGetInt64(out var value)) {{ return false; }}                            
                        result.{declinfo.DeclSymbol.Name} = value;
                        ");
                        }
                        else
                        {
                            builder.Append($@"
                        if (!reader.TryGetInt64(out result.{declinfo.DeclSymbol.Name})) {{ return false; }}                            
                        ");
                        }

                        break;
                    }
                default:
                    {
                        if (declinfo.IsGenericList && !declinfo.GenericType.Name.Equals("BsonDocument"))
                        {
                            builder.Append(GenerateReadArray(info, declinfo));
                            break;
                        }
                        if (declinfo.IsGenericList && declinfo.GenericType.Name.Equals("BsonDocument"))
                        {
                            builder.Append(GenerateReadArrayToBsonDocumentList(info, declinfo));
                            break;
                        }
                        if (declinfo.IsClassOrStruct)
                        {
                            builder.Append(GenerateReadOtherDocument(info, declinfo));
                            break;
                        }

                        throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(GenerateReadsAndAssign)} with type name {declinfo.DeclType.Name}");
                    }


            }
            return builder.ToString();
        }
        private string GenerateReadArrayToBsonDocumentList(ClassDecl classdecl, MemberDeclarationInfo memberdecl)
        {
            StringBuilder builder = new StringBuilder();
            if (memberdecl.IsProperty)
            {
                builder.Append($@"                    
                     if ( !reader.TryGetArrayAsDocumentList(out var docList)){{ return false;}}            
                     result.{memberdecl.DeclSymbol.Name} = docList;
            ");
            }
            else
            {
                builder.Append($@"                    
                     if ( !reader.TryGetArrayAsDocumentList(out result.{memberdecl.DeclSymbol.Name})){{ return false;}}            
            ");
            }

            return builder.ToString();
        }
        private string GenerateReadArray(ClassDecl classdecl, MemberDeclarationInfo memberdecl)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                    result.{memberdecl.DeclSymbol.Name} = new List<{memberdecl.GenericType.Name}>();
                    if (!reader.TryGetInt32(out var arrayDocLength)) {{ return false; }}
                    var arrayUnreaded = reader.Remaining + sizeof(int);
                    while (arrayUnreaded - reader.Remaining < arrayDocLength - 1)
                    {{
                        if ( !reader.TryGetCStringAsSpan(out var index)) {{ return false; }}
                        if ( !GlobalSerializationHelperGenerated.{memberdecl.GenericType.Name}GeneratedSerializatorStaticField.TryParse(ref reader, out var arrayElement)){{ return false;}}
                        result.{memberdecl.DeclSymbol.Name}.Add(arrayElement as {memberdecl.GenericType.Name});
                    }}
                    if ( !reader.TryGetByte(out var arrayEndMarker)){{ return false; }}
                    if (arrayEndMarker != '\x00')
                    {{
                        throw new ArgumentException($""{classdecl.ClassSymbol.Name}GeneratedSerializator.TryParse End document marker missmatch"");
                    }}             
        ");
            return builder.ToString();
        }
        private string GenerateReadOtherDocument(ClassDecl classdecl, MemberDeclarationInfo memberdecl)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                     if ( !GlobalSerializationHelperGenerated.{memberdecl.DeclType.Name}GeneratedSerializatorStaticField.TryParse(ref reader, out var value)){{ return false;}}
                     result.{memberdecl.DeclSymbol.Name} = ({memberdecl.DeclType.Name})value;
            ");
            return builder.ToString();
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
                var info = new ClassDecl(symbol);
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
                                info.MemberDeclarations.Add(new MemberDeclarationInfo(fieldSymbol));
                            }
                        }
                    }
                    if (member is PropertyDeclarationSyntax propDecl)
                    {
                        SemanticModel memberModel = context.Compilation.GetSemanticModel(propDecl.SyntaxTree);

                        ISymbol propertySymbol = memberModel.GetDeclaredSymbol(propDecl);
                        if (propertySymbol.DeclaredAccessibility == Accessibility.Public)
                        {
                            info.MemberDeclarations.Add(new MemberDeclarationInfo(propertySymbol));
                        }
                    }
                }
                meta.Add(info);
            }
        }
        private string GenerateStaticReadOnlyBsonFieldsSpans(ClassDecl info)
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
