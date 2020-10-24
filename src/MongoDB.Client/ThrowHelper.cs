using MongoDB.Client.Protocol.Common;
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
            throw new Exception("Cant connect to endpoint: " + endpoint.ToString()); // TODO: custom excention
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedTypeException<T>(Type type)
        {
            throw new Exception("Unsupported type: " + type.ToString()); // TODO: custom excention
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
    }
}
