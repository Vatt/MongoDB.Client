using MongoDB.Client.Bson.Generators.SyntaxGenerator;
using System.Text;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator;

namespace MongoDB.Client.Bson.Generators
{
    partial class BsonSerializerGenerator
    {
        public string GenerateGlobalHelperStaticClass(MasterContext ctx)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                using MongoDB.Client.Bson.Reader;
                using MongoDB.Client.Bson.Serialization;
                using System;
                using System.Collections.Generic;
                using System.Runtime.CompilerServices;

                namespace MongoDB.Client.Bson.Serialization.Generated{{
                    public static class GlobalSerializationHelperGenerated{{
                        {GenerateFields()}
                        {GenerateGetSeriazlizersMethod()}

                        [ModuleInitializerAttribute]
                        public static void MapInit()
                        {{
                            MongoDB.Client.Bson.Serialization.SerializersMap.RegisterSerializers(GetGeneratedSerializers()) ;
                        }}
                    }}
                }}");
            return builder.ToString();
            string GenerateFields()
            {
                var builder = new StringBuilder();
                foreach (var context in ctx.Contexts)
                {
                    if (context.GenericArgs.HasValue)
                    {
                        continue;
                    }
                    builder.Append($"\n\t\tpublic static readonly  IGenericBsonSerializer<{context.Declaration.ToString()}>  {SerializerGenerator.SerializerName(context)}StaticField = new {SerializerGenerator.SerializerName(context)}();");
                }
                return builder.ToString();
            }
            string GenerateGetSeriazlizersMethod()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($@"
                public static KeyValuePair<Type, IBsonSerializer>[]  GetGeneratedSerializers()
                {{
                    var pairs = new List<KeyValuePair<Type, IBsonSerializer>>();                    
                ");
                int index = 0;
                foreach (var context in ctx.Contexts)
                {
                    if (context.GenericArgs.HasValue)
                    {
                        continue;
                    }
                    builder.Append($@"
                    pairs.Add(KeyValuePair.Create<Type, IBsonSerializer>(typeof({context.Declaration.ToString()}), {SerializerGenerator.SerializerName(context)}StaticField));
                    ");
                    index++;
                }
                builder.Append(@"
                    return pairs.ToArray();
                }
                ");
                return builder.ToString();
            }
        }
    }
}
