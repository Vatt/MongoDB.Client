using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    partial class BsonSerializerGenerator
    {
        public string GenerateGlobalHelperStaticClass()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                using MongoDB.Client.Bson.Reader;
                using MongoDB.Client.Bson.Serialization;
                using System;
                using System.Collections.Generic;
                using System.Runtime.CompilerServices;

                namespace MongoDB.Client.Bson.Serialization.Generated{{
                    public static class {Basics.GlobalSerializationHelperGeneratedString}{{
                        {GenerateFields()}
                        {GenerateGetSeriazlizersMethod()}

                        [ModuleInitializerAttribute]
                        public static void MapInit()
                        {{
                            SerializersMap.RegisterSerializers(GetGeneratedSerializers()) ;
                        }}
                    }}
                }}");
            return builder.ToString();
            string GenerateFields()
            {
                var builder = new StringBuilder();
                foreach (var info in meta)
                {
                    builder.Append($"\n\t\tpublic static readonly  IGenericBsonSerializer<{info.ClassSymbol.ToString()}>  {Basics.GenerateSerializerNameStaticField(info.ClassSymbol)} = new {Basics.GenerateSerializerName(info.ClassSymbol)}();");
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
                    pairs[{index}] = KeyValuePair.Create<Type, IBsonSerializer>(typeof({decl.ClassSymbol.ToString()}), {Basics.GenerateSerializerNameStaticField(decl.ClassSymbol)});
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
    }
}
