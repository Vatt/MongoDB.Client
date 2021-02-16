using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInt32(out int? value)
        {
            value = default;
            if(_input.TryReadLittleEndian(out int temp))
            {
                value = temp;
                return true;
            }
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInt64(out long? value)
        {
            value = default;
            if(_input.TryReadLittleEndian(out long temp))
            {
                value = temp;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDouble(out double? value)
        {
            if (TryGetInt64(out long temp))
            {
                value = BitConverter.Int64BitsToDouble(temp);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBoolean(out bool? value)
        {
            if (TryGetByte(out var boolean))
            {
                value = boolean == 1;
                return true;

            }

            value = default;
            return false;
        }
    }
}