using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace MongoDB.Client
{
    internal static class ThrowHelper
    {
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
    }
}
