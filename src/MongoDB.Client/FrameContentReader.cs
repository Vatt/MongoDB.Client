using System;
using System.Buffers;

namespace MongoDB.Client
{

    internal class FrameContentReader
    {
        private ReplyMessage _message;
        public ReplyMessage Message
        {
            get
            {
                if (_readState is ReadState.Complete)
                {
                    return _message;
                }
                ThrowHelper.ReadInProgressException();
                return null;
            }
        }

        private ReadState _readState;
        private int _remainingBytes;
       

        public bool TryParseMessage(ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined)
        {
            var reader = new SequenceReader<byte>(input);

            if (_readState == ReadState.ReadHeader)
            {
                // The header consists of
                //
                // [4 bytes]  [4 bytes  ]  [4 bytes   ]  [4 bytes]  [4 bytes      ]  [8 bytes ]  [4 bytes     ]  [4 bytes       ]
                // [size   ]  [requestId]  [responseTo]  [opcode ]  [responseFlags]  [cursorId]  [startingFrom]  [numberReturned]

                if (TryReadOpcode(ref reader, out var opcode) == false)
                {
                    return false;
                }
                if (opcode is Opcode.Reply || opcode is Opcode.OpMsg)
                {
                    reader.TryReadLittleEndian(out int messageSize);
                    reader.TryReadLittleEndian(out int requestId);
                    reader.TryReadLittleEndian(out int responseTo);
                    reader.TryReadLittleEndian(out int _); // opcode
                    if (reader.TryReadLittleEndian(out int responseFlags) == false)
                    {
                        return false;
                    }
                    if (reader.TryReadLittleEndian(out long cursorId) == false)
                    {
                        return false;
                    }
                    if (reader.TryReadLittleEndian(out int startingFrom) == false)
                    {
                        return false;
                    }
                    if (reader.TryReadLittleEndian(out int numberReturned) == false)
                    {
                        return false;
                    }

                    var flags = (ResponseFlags)responseFlags;
                    var awaitCapable = (flags & ResponseFlags.AwaitCapable) == ResponseFlags.AwaitCapable;
                    var cursorNotFound = (flags & ResponseFlags.CursorNotFound) == ResponseFlags.CursorNotFound;
                    var queryFailure = (flags & ResponseFlags.QueryFailure) == ResponseFlags.QueryFailure;

                    if (reader.TryReadLittleEndian(out int payloadSize) == false)
                    {
                        return false;
                    }
                    reader.Rewind(sizeof(int));

                    _message = new ReplyMessage
                    {
                        RequestId = requestId,
                        ResponseTo = responseTo,
                        AwaitCapable = awaitCapable,
                        CursorId = cursorId,
                        CursorNotFound = cursorNotFound,
                        NumberReturned = numberReturned,
                        QueryFailure = queryFailure,
                        StartingFrom = startingFrom
                    };

                    if (payloadSize == 0)
                    {
                        _readState = ReadState.Complete;
                    }
                    else
                    {
                        _readState = queryFailure == false ? ReadState.ReadPayload : ReadState.ReadQueryFailure;
                        _remainingBytes = payloadSize;
                        var payload = MemoryPool<byte>.Shared.Rent(payloadSize);
                        _message.Payload = payload;
                        _message.PayloadSize = payloadSize;
                        consumed = reader.Position;
                        examined = reader.Position;
                    }
                }
                else
                {
                    ThrowHelper.WrongOpcodeException();
                }
            }

            if (_readState == ReadState.ReadPayload)
            {
                var readCount = Math.Min(_remainingBytes, (int)reader.Remaining);
                var offset = _message.PayloadSize - _remainingBytes;
                var payloadSpan = _message.Payload!.Memory.Span.Slice(offset, readCount);
                if (reader.TryCopyTo(payloadSpan))
                {
                    reader.Advance(readCount);
                    _remainingBytes -= readCount;
                }

                consumed = reader.Position;
                examined = reader.Position;

                if (_remainingBytes == 0)
                {
                    _readState = ReadState.Complete;
                }
            }

            return _readState == ReadState.Complete;
        }

        private bool TryReadOpcode(ref SequenceReader<byte> reader, out Opcode opcode)
        {
            opcode = Opcode.Reply;
            if (reader.Length < 16)
            {
                return false;
            }

            reader.Advance(12);
            if (!reader.TryReadLittleEndian(out int opcodeValue))
            {
                return false;
            }
            reader.Rewind(16);
            opcode = (Opcode)opcodeValue;
            return true;
        }


        public void Reset()
        {
            _readState = ReadState.ReadHeader;
            _message = null!;
        }

        private enum ReadState
        {
            ReadHeader, ReadPayload, ReadQueryFailure, Complete
        }


        [Flags]
        private enum ResponseFlags
        {
            None = 0,
            CursorNotFound = 1,
            QueryFailure = 2,
            AwaitCapable = 8
        }
    }
}
