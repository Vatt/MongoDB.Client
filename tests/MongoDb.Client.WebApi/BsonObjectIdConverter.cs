using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Client.Bson.Document;

namespace MongoDb.Client.WebApi
{
    public class BsonObjectIdConverter : JsonConverter<BsonObjectId>
    {
        public override BsonObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Span<char> span = stackalloc char[24];
            Encoding.UTF8.GetChars(reader.ValueSpan, span);
            var result = new BsonObjectId(span);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, BsonObjectId value, JsonSerializerOptions options)
        {
            Span<char> span = stackalloc char[24];
            var chars = MemoryMarshal.Cast<char, byte>(span);
            value.TryFormat(span, out _);
            writer.WriteStringValue(span);
        }
    }
}
