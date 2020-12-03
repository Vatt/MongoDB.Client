using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client.Tests.Serialization
{
   
    public abstract class BaseSerialization 
    {
        private delegate bool TryParseDelegate<T>(ref BsonReader reader, out T message);
        private delegate void WriteDelegate<T>(ref BsonWriter write, in T message);
        private struct Serializer<T> : IGenericBsonSerializer<T>
        {
            private TryParseDelegate<T> _reader;
            private WriteDelegate<T> _writer;
            public Serializer(TryParseDelegate<T> reader, WriteDelegate<T> writer)
            {
                _reader = reader;
                _writer = writer;
            }
            public bool TryParse(ref BsonReader reader, [MaybeNullWhen(false)] out T message)
            {
                return _reader(ref reader, out message);
            }

            public void Write(ref BsonWriter writer, in T message)
            {
                _writer(ref writer, message);
            }
        }

        private static TryParseDelegate<T> GetTryParseDelegate<T>()
        {
            return typeof(T).GetMethod("TryParse").CreateDelegate(typeof(TryParseDelegate<T>)) as TryParseDelegate<T>;
        }
        private static WriteDelegate<T> GetWriteDelegate<T>()
        {
            return typeof(T).GetMethod("Write").CreateDelegate(typeof(WriteDelegate<T>)) as WriteDelegate<T>;
        }
        public static async Task<T> RoundTripAsync<T>(T message)
        {
            var pipe = new Pipe(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue));
            var wtask = WriteAsync(pipe.Writer, message);
            var rtask = ReadAsync<T>(pipe.Reader);
            await wtask;
            return await rtask;
        }
        public static async Task<T> RoundTripAsync<T>(T message, IGenericBsonSerializer<T> serializer)
        {
            var pipe = new Pipe(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue));
            var wtask = WriteAsync(pipe.Writer, message, serializer);
            var rtask = ReadAsync<T>(pipe.Reader, serializer);
            await wtask;
            return await rtask;
        }

        public static async Task<BsonDocument> RoundTripWithBsonAsync<T>(T message)
        {
            var pipe = new Pipe();
            await WriteAsync(pipe.Writer, message);
            return await ReadAsync<BsonDocument>(pipe.Reader, new BsonDocumentSerializer());
        }

        public static async Task<T1> RoundTripAsync<T0, T1>(T0 message)
        {
            var pipe = new Pipe();
            var writer = new Serializer<T0>(GetTryParseDelegate<T0>(), GetWriteDelegate<T0>());
            var reader = new Serializer<T1>(GetTryParseDelegate<T1>(), GetWriteDelegate<T1>());
            await WriteAsync(pipe.Writer, message);
            return await ReadAsync(pipe.Reader, reader);
        }
        public static async Task<T> ReadAsync<T>(PipeReader input, IGenericBsonSerializer<T> serializer)
        {
            var reader = new ProtocolReader(input);
            var messageReader = new ReplyBodyReader<T>(serializer, new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }
        public static async Task<T> ReadAsync<T>(PipeReader input)
        {
            var reader = new ProtocolReader(input);
            var serializer = new Serializer<T>(GetTryParseDelegate<T>(), GetWriteDelegate<T>());
            var messageReader =  new ReplyBodyReader<T>(serializer, new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }


        public static async Task WriteAsync<T>(PipeWriter output, T message)
        {
            var writer = new ProtocolWriter(output);
            var serializer = new Serializer<T>(GetTryParseDelegate<T>(), GetWriteDelegate<T>());
            var messageWriter = new ReplyBodyWriter<T>(serializer);
            await writer.WriteAsync(messageWriter, message).ConfigureAwait(false);
            await output.FlushAsync();
            await output.CompleteAsync();
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
