using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    internal partial class Channel
    {
        internal static class InsertParserCallbackHolder<T>
        {
            
            internal class InsertMsgType0BodyReaderUnsafe : IMessageReader<InsertResult>
            {
                private readonly static unsafe delegate*<ref BsonReader, out InsertResult, bool> TryParseFnPtr;
                unsafe static InsertMsgType0BodyReaderUnsafe()
                {
                    TryParseFnPtr = SerializerFnPtrProvider<InsertResult>.TryParseFnPtr;
                }
                public long Consumed { get; private set; }

                public unsafe bool TryParseMessage(
                    in ReadOnlySequence<byte> input,
                    ref SequencePosition consumed,
                    ref SequencePosition examined,
                    [MaybeNullWhen(false)] out InsertResult message)
                {
                    var bsonReader = new BsonReader(input);


                    if (TryParseFnPtr(ref bsonReader, out message) == false)
                    {
                        return false;
                    }

                    consumed = bsonReader.Position;
                    examined = bsonReader.Position;
                    Consumed = bsonReader.BytesConsumed;

                    return true;
                }
            }

            public static Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>>? Parser;
            public static Func<int, ParserCompletion>? Completion;
            public static readonly IMessageReader<InsertResult> InsertBodyReader = new InsertMsgType0BodyReaderUnsafe();
        }
    }

}
