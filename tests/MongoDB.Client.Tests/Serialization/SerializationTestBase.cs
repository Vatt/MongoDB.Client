using System;
using System.Buffers;
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
    internal unsafe class UnitTestReplyBodyWriter<T> : IMessageWriter<T>
        where T: IBsonSerializer<T>
    {
        public void WriteMessage(T message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            //SerializerFnPtrProvider<T>.WriteFnPtr(ref writer, message);
            T.WriteBson(ref writer, message);

        }
    }
    internal unsafe class UnitTestReplyBodyReader<T> : IMessageReader<QueryResult<T>>
        where T: IBsonSerializer<T>
    {
        private readonly ReplyMessage _replyMessage;
        private readonly QueryResult<T> _result;

        public UnitTestReplyBodyReader(ReplyMessage replyMessage)
        {
            _replyMessage = replyMessage;
            _result = new QueryResult<T>(_replyMessage.ReplyHeader.CursorId);
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out QueryResult<T> message)
        {
            message = _result;
            var bsonReader = new BsonReader(input);
            while (_result.Count < _replyMessage.ReplyHeader.NumberReturned)
            {
                //if (SerializerFnPtrProvider<T>.TryParseFnPtr(ref bsonReader, out var item))
                if (T.TryParseBson(ref bsonReader, out var item))
                {
                    _result.Add(item);
                    consumed = bsonReader.Position;
                    examined = bsonReader.Position;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
    public abstract class SerializationTestBase
    {
        public static async Task<T> RoundTripAsync<T>(T message) where T : IBsonSerializer<T>
        {
            var pipe = new Pipe(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue));
            var wtask = WriteAsync(pipe.Writer, message);
            var rtask = ReadAsync<T>(pipe.Reader);
            await wtask;
            return await rtask;
        }
        public static async Task<BsonDocument> RoundTripWithBsonAsync<T>(T message) where T : IBsonSerializer<T>
        {
            var pipe = new Pipe();
            await WriteAsync(pipe.Writer, message);
            return await ReadAsync<BsonDocument>(pipe.Reader);
        }

        public static async Task<T1> RoundTripAsync<T0, T1>(T0 message)
            where T0 : IBsonSerializer<T0>
            where T1 : IBsonSerializer<T1>
        {
            var pipe = new Pipe();
            UnitTestReplyBodyReader<T1> reader = default;
            unsafe
            {
                reader = new UnitTestReplyBodyReader<T1>(new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            }
            await WriteAsync(pipe.Writer, message);
            return await ReadAsync<T1>(pipe.Reader, reader);
        }
        internal static async Task<T> ReadAsync<T>(PipeReader input, UnitTestReplyBodyReader<T> messageReader) where T : IBsonSerializer<T>
        {
            var reader = new ProtocolReader(input);
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }
        internal static async Task<T> ReadAsync<T>(PipeReader input) where T : IBsonSerializer<T>
        {
            var reader = new ProtocolReader(input);
            var messageReader = new ReplyBodyReader<T>(new ReplyMessage(default, new ReplyMessageHeader(default, default, default, 1)));
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message.FirstOrDefault();
        }
        public static async Task WriteAsync<T>(PipeWriter output, T message) where T: IBsonSerializer<T>
        {
            var writer = new ProtocolWriter(output);
            UnitTestReplyBodyWriter<T> messageWriter = default;
            unsafe
            {
                messageWriter = new UnitTestReplyBodyWriter<T>();
            }
            await writer.WriteUnsafeAsync(messageWriter, message).ConfigureAwait(false);
            await output.FlushAsync();
            await output.CompleteAsync();
        }
    }
}
