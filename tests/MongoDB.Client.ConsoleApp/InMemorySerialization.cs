using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client.ConsoleApp
{

    public static class InMemorySerialization 
    {
        public static async Task<T> RoundTripAsync<T>(T message)
        {
            var pipe = new Pipe(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue));
            SerializersMap.TryGetSerializer<T>(out var serializer);
            var wtask = WriteAsync(pipe.Writer, message, serializer);
            var rtask = ReadAsync(pipe.Reader, serializer);
            await wtask;
            return await rtask;
        }

        public static async Task<BsonDocument> RoundTripToBsonAsync<T>(T message)
        {
            var pipe = new Pipe();
            SerializersMap.TryGetSerializer<T>(out var serializer);
            await WriteAsync(pipe.Writer, message, serializer);
            return await ReadAsync<BsonDocument>(pipe.Reader, new BsonDocumentSerializer());
        }

        public static async Task<TReader> RoundTripAsync<TWriter, TReader>(TWriter message)
        {
            var pipe = new Pipe();
            SerializersMap.TryGetSerializer<TWriter>(out var writerSerializer);
            SerializersMap.TryGetSerializer<TReader>(out var readerSerializer);
            await WriteAsync(pipe.Writer, message, writerSerializer);
            return await ReadAsync(pipe.Reader, readerSerializer);
        }
        
        public static async Task<T> ReadAsync<T>(PipeReader input, IGenericBsonSerializer<T> serializer)
        {
            var reader = new ProtocolReader(input);

            var messageReader =  new ReplyBodyReader<T>(serializer, new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
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
