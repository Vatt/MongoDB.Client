using System.IO.Pipelines;
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
        public static async Task<T> RoundTripAsync<T>(T message) where T : IBsonSerializer<T>
        {
            var pipe = new Pipe(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue));
            var wtask = WriteAsync<T>(pipe.Writer, message);
            var rtask = ReadAsync<T>(pipe.Reader);
            await wtask;
            return await rtask;
        }

        public static async Task<BsonDocument> RoundTripToBsonAsync<T>(T message) where T : IBsonSerializer<T>
        {
            var pipe = new Pipe();
            await WriteAsync<T>(pipe.Writer, message);
            return await ReadAsync<BsonDocument>(pipe.Reader);
        }
        public static async Task<T> ReadAsync<T>(PipeReader input) where T : IBsonSerializer<T>
        {
            var reader = new ProtocolReader(input);

            var messageReader = new ReplyBodyReader<T>(new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }


        public static async Task WriteAsync<T>(PipeWriter output, T message) where T : IBsonSerializer<T>
        {
            var writer = new ProtocolWriter(output);

            var messageWriter = new ReplyBodyWriter<T>();
            await writer.WriteAsync(messageWriter, message).ConfigureAwait(false);
            await output.FlushAsync();
            await output.CompleteAsync();
        }
    }
}
