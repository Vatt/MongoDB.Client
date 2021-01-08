﻿using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks.Serialization
{
    class SerializationHelper
    {
        public static async ValueTask<T> RoundTripAsync<T>(T message, IGenericBsonSerializer<T> serializer)
        {
            var pipe = new Pipe();

            await WriteAsync(pipe.Writer, message, serializer);
            return await ReadAsync(pipe.Reader, serializer);
        }

        private static async ValueTask<T> ReadAsync<T>(PipeReader input, IGenericBsonSerializer<T> serializer)
        {
            var reader = new ProtocolReader(input);

            var messageReader = new ReplyBodyReader<T>(serializer, new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }


        private static async ValueTask WriteAsync<T>(PipeWriter output, T message, IGenericBsonSerializer<T> serializer)
        {
            var writer = new ProtocolWriter(output);

            var messageWriter = new ReplyBodyWriter<T>(serializer);
            await writer.WriteUnsafeAsync(messageWriter, message).ConfigureAwait(false);
        }
    }
}
