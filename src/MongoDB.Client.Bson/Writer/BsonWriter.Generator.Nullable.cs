using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using System;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MongoDB.Client.Bson.Writer
{

    public ref partial struct BsonWriter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, double? value)
        {
            if(value.HasValue)
            {
                WriteByte(1);
                WriteCString(name);
                WriteDouble(value.Value);
            }
            else
            {
                WriteBsonNull(name);
            }

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, double? value)
        {
            if(value.HasValue)
            {
                WriteByte(1);
                WriteIntIndex(intName);
                WriteDouble(value.Value);
            }
            else
            {
                WriteBsonNull(intName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, bool? value)
        {
            if (value.HasValue)
            {
                WriteByte(8);
                WriteCString(name);
                WriteBoolean(value.Value);
            }
            else
            {
                WriteBsonNull(name);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, bool? value)
        {
            if(value.HasValue)
            {
                WriteByte(8);
                WriteIntIndex(intName);
                WriteBoolean(value.Value);
            }
            else
            {
                WriteBsonNull(intName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, int? value)
        {
            if(value.HasValue)
            {
                WriteByte(16);
                WriteCString(name);
                WriteInt32(value.Value);
            }
            else
            {
                WriteBsonNull(name);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, int? value)
        {
            if(value.HasValue)
            {
                WriteByte(16);
                WriteIntIndex(intName);
                WriteInt32(value.Value);
            }
            else
            {
                WriteBsonNull(intName);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(ReadOnlySpan<byte> name, long? value)
        {
            if(value.HasValue)
            {
                WriteByte(18);
                WriteCString(name);
                WriteInt64(value.Value);
            }
            else
            {
                WriteBsonNull(name);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write_Type_Name_Value(int intName, long? value)
        {
            if(value.HasValue)
            {
                WriteByte(18);
                WriteIntIndex(intName);
                WriteInt64(value.Value);
            }
            else
            {
                WriteBsonNull(intName);
            }
        }
    }
}
