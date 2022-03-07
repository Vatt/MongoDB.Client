using System.IO.Pipelines;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client.Benchmarks.Serialization
{
    class SerializationHelper
    {
        public static async ValueTask<T> RoundTripAsync<T>(T message) where T : IBsonSerializer<T>
        {
            var pipe = new Pipe();

            await WriteAsync(pipe.Writer, message);
            return await ReadAsync<T>(pipe.Reader);
        }

        private static async ValueTask<T> ReadAsync<T>(PipeReader input) where T : IBsonSerializer<T>
        {
            var reader = new ProtocolReader(input);

            var messageReader = new ReplyBodyReader<T>(new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }


        private static async ValueTask WriteAsync<T>(PipeWriter output, T message) where T : IBsonSerializer<T>
        {
            var writer = new ProtocolWriter(output);

            var messageWriter = new ReplyBodyWriter<T>();
            //await writer.WriteUnsafeAsync(messageWriter, message).ConfigureAwait(false);
            await writer.WriteAsync(messageWriter, message).ConfigureAwait(false);
        }
    }
}
