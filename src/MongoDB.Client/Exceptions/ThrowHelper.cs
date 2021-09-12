using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Common;

namespace MongoDB.Client.Exceptions
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T ConnectionTerminated<T>()
        {
            throw new InvalidDataException("Connection terminated while reading a message.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T MissedAdvance<T>()
        {
            throw new InvalidOperationException("Advance must be called before calling ReadAsync");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T WrongOpcodeException<T>()
        {
            throw new FormatException("Reply message opcode is not OP_REPLY.");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T ReadInProgressException<T>()
        {
            throw new InvalidOperationException("Response read in progress");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T ConnectionException<T>(System.Net.EndPoint endpoint)
        {
            throw new MongoException("Cant connect to endpoint: " + endpoint.ToString()); // TODO: custom excention
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedTypeException<T>(Type type)
        {
            throw new MongoException("Unsupported type: " + type.ToString()); // TODO: custom excention
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T ObjectDisposedException<T>(string name)
        {
            throw new ObjectDisposedException(name);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T OpcodeNotSupportedException<T>(Opcode opcode)
        {
            throw new NotSupportedException($"Opcode '{opcode}' not supported");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T InvalidPayloadTypeException<T>(int payloadType)
        {
            throw new NotSupportedException($"Command message invalid payload type: {payloadType}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnknownCursorFieldException<T>(string fieldName)
        {
            throw new NotSupportedException($"Unknown cursor field: {fieldName}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T MissedDocumentEndMarkerException<T>()
        {
            throw new NotSupportedException($"Document end marker was not found");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void InvalidProtocolHeader()
        {
            throw new InvalidDataException("Invalid AMQP protocol header from server");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void PacketNotRecognized(int transportHigh, int transportLow, int serverMajor, int serverMinor)
        {
            throw new InvalidDataException("Invalid protocol version");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void InvalidFrameEndMarker(int endMarker)
        {
            throw new InvalidDataException("Bad frame end marker: " + endMarker);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ConnectionTerminated()
        {
            throw new InvalidDataException("Connection terminated while reading a message.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void MissedAdvance()
        {
            throw new InvalidOperationException("Advance must be called before calling ReadAsync");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void WrongOpcodeException()
        {
            throw new FormatException("Reply message opcode is not OP_REPLY.");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ReadInProgressException()
        {
            throw new InvalidOperationException("Response read in progress");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ConnectionException(System.Net.EndPoint endpoint)
        {
            throw new Exception("Cant connect to endpoint: " + endpoint.ToString()); // TODO: custom excention
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void UnsupportedTypeException(Type type)
        {
            throw new Exception("Unsupported type: " + type.ToString()); // TODO: custom excention
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ObjectDisposedException(string name)
        {
            throw new ObjectDisposedException(name);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void CancelledException()
        {
            throw new OperationCanceledException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void CursorException(string message)
        {
            throw new MongoCursorException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void InsertException(string message)
        {
            throw new MongoInsertException(message);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void InsertException(List<InsertError> errors)
        {
            throw new MongoInsertException(errors);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void DropCollectionException(string errors, int code, string codename)
        {
            throw new MongoCommandException(errors, code, codename);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void CreateCollectionException(string errorMessage, int code, string codename)
        {
            throw new MongoCommandException(errorMessage, code, codename);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void TransactionException(string errorMessage, int code, string codename)
        {
            throw new MongoCommandException(errorMessage, code, codename);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void InvalidBsonException()
        {
            throw new MongoException("Invalid bson");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ThrowNotInitialized()
        {
            throw new MongoException("Client must be initialized");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T InvalidReturnType<T>(Type expected, Type actual)
        {
            throw new MongoException($"Expected '{expected}' bat was '{actual}'");
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T MongoInitExceptions<T>()
        {
            throw new MongoException($"Connection failed");
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void PrimaryNullExceptions()
        {
            throw new MongoException($"Connection failed: Prymary is null");
        }
    }
}
