using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Readers;
using MongoDB.Client.Writers;

namespace MongoDB.Client.Tests.Serialization
{

    public abstract class BaseSerialization
    {
        public static async Task<T> RoundTripAsync<T>(T message, IGenericBsonSerializer<T> serializer)
        {
            var pipe = new Pipe();

            await WriteAsync(pipe.Writer, message, serializer);
            return await ReadAsync(pipe.Reader, serializer);
        }

        public static async Task<BsonDocument> RoundTripWithBsonAsync<T>(T message, IGenericBsonSerializer<T> serializer)
        {
            var pipe = new Pipe();

            await WriteAsync(pipe.Writer, message, serializer);
            return await ReadAsync<BsonDocument>(pipe.Reader, new BsonDocumentSerializer());
        }

        public static async Task<T1> RoundTripAsync<T0, T1>(T0 message, IGenericBsonSerializer<T0> writer, IGenericBsonSerializer<T1> reader)
        {
            var pipe = new Pipe();

            await WriteAsync(pipe.Writer, message, writer);
            return await ReadAsync(pipe.Reader, reader);
        }
        public static async Task<T> ReadAsync<T>(PipeReader input, IGenericBsonSerializer<T> serializer)
        {
            var reader = new ProtocolReader(input);

            var messageReader =  new ReplyBodyReader<T>(serializer, new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return messageReader.Result.FirstOrDefault();
        }


        public static async Task WriteAsync<T>(PipeWriter output, T message, IGenericBsonSerializer<T> serializer)
        {
            var writer = new ProtocolWriter(output);

            var messageWriter = new ReplyBodyWriter<T>(serializer);
            await writer.WriteAsync(messageWriter, message).ConfigureAwait(false);
            await output.FlushAsync();
            await output.CompleteAsync();
        }
    }
}
