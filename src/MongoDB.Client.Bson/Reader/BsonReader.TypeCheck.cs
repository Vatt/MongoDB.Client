using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
    	public bool TryRead(BsonType type, out double value)
    	{
            value = default;
            switch(type)
            {
                case BsonType.Double:
                    return TryGetDouble(out value);
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp) is false)
                        {
                            return false;
                        }

                        value = temp;
                        return true;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp) is false)
                        {
                            return false;
                        }

                        value = temp;
                        return true;
                    }
                case BsonType.String:
                    {
                        if (TryGetStringAsSpan(out var temp) is false)
                        {
                            return false;
                        }

                        if (double.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                        {
                            return ThrowHelper.InvalidTypeException(BsonType.Double, BsonType.String, Encoding.UTF8.GetString(temp));
                        }

                        return true;
                    }
                case BsonType.Null:
                    value = default;
                    return true;
                default:
                    return ThrowHelper.InvalidTypeException(BsonType.Double, type);
            }
    	}
        public bool TryRead(BsonType type, out double? value)
        {
            if (TryRead(type, out double temp) is false)
            {
                value = null;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out string? value)
        {
            if (type is BsonType.String)
            {
                return TryGetString(out value);
            }
            else if (type is BsonType.Null)
            {
                value = null;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.String, type);
        }
        public bool TryRead(BsonType type, out BsonDocument? value)
        {
            if (type is BsonType.Document)
            {
                return TryParseDocument(out value);
            }
            else if (type is BsonType.Null)
            {
                value = default;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.Document, type);
        }
        public bool TryRead(BsonType type, out BsonArray? value)
        {
            if (type is BsonType.Array)
            {
                return TryGetArray(out value);
            }
            else if (type is BsonType.Null)
            {
                value = default;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.Document, type);
        }
        public bool TryRead(BsonType type, out BsonBinaryData value)
        {
            if (type is BsonType.BinaryData)
            {
                return TryGetBinaryData(out value);
            }
            else if (type is BsonType.Null)
            {
                value = default;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.BinaryData, type);
        }
        public bool TryRead(BsonType type, out BsonBinaryData? value)
        {
            if (TryRead(type, out BsonBinaryData temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out BsonObjectId value)
        {
            if (type is BsonType.ObjectId)
            {
                return TryGetObjectId(out value);
            }
            else if (type is BsonType.Null)
            {
                value = default;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.ObjectId, type);
        }
        public bool TryRead(BsonType type, out BsonObjectId? value)
        {
            if (TryRead(type, out BsonObjectId temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out bool value)
        {
            switch (type)
            {
                case BsonType.Boolean:
                    return TryGetBoolean(out value);
                case BsonType.Double:
                    {
                        if (TryGetDouble(out double temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        value = temp is not 0.0;
                        return true;
                    }
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        value = temp is not 0;
                        return true;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        value = temp is not 0;
                        return true;
                    }
                case BsonType.Null:
                    {
                        value = false;
                        return true;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Boolean, type);

            }
        }
        public bool TryRead(BsonType type, out bool? value)
        {
            if (TryRead(type, out bool temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out DateTimeOffset value)
        {
            switch (type)
            {
                case BsonType.UtcDateTime or BsonType.Int64:
                    {
                        return TryGetUtcDatetime(out value);
                    }
                case BsonType.Document:
                    {
                        return TryGetDatetimeFromDocument(out value);
                    }
                case BsonType.String:
                    {
                        if (TryGetStringAsSpan(out var temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        if (DateTimeOffset.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                        {
                            return ThrowHelper.InvalidTypeException(BsonType.UtcDateTime, BsonType.String, Encoding.UTF8.GetString(temp));
                        }

                        return true;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.UtcDateTime, type);
            }
        }
        public bool TryRead(BsonType type, out DateTimeOffset? value)
        {
            if (TryRead(type, out DateTimeOffset temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out int value)
        {
            switch (type)
            {
                case BsonType.Int32:
                    return TryGetInt32(out value);
                case BsonType.Null:
                    value = default;
                    return true;
                case BsonType.String:
                    {
                        if (TryGetCStringAsSpan(out var temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        if (int.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                        {
                            return ThrowHelper.InvalidTypeException(BsonType.Int32, BsonType.String, Encoding.UTF8.GetString(temp));
                        }

                        return true;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Int32, type);
            }
        }
        public bool TryRead(BsonType type, out int? value)
        {
            if (TryRead(type, out int temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out BsonTimestamp value)
        {
            if (type is BsonType.Timestamp)
            {
                return TryGetTimestamp(out value);
            }
            else if (type is BsonType.Null)
            {
                value = default;
                return true;
            }

            value = default;
            return ThrowHelper.InvalidTypeException(BsonType.Timestamp, type);
        }
        public bool TryRead(BsonType type, out BsonTimestamp? value)
        {
            if (TryRead(type, out BsonTimestamp temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out long value)
        {
            switch (type)
            {
                case BsonType.Int64:
                    return TryGetInt64(out value);
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        value = temp;
                        return true;
                    }
                case BsonType.Null:
                    value = default;
                    return true;
                case BsonType.String:
                    {
                        if (TryGetCStringAsSpan(out var temp) is false)
                        {
                            value = default;
                            return false;
                        }

                        if (long.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                        {
                            return ThrowHelper.InvalidTypeException(BsonType.Int32, BsonType.String, Encoding.UTF8.GetString(temp));
                        }

                        return true;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Int32, type);
            }
        }
        public bool TryRead(BsonType type, out long? value)
        {
            if (TryRead(type, out long temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryRead(BsonType type, out decimal value)
        {
            value = default;
            switch (type)
            {
                case BsonType.Double:
                    {
                        if (TryGetDouble(out double doubleValue))
                        {
                            value = new(doubleValue);

                            return true;
                        }

                        return false;
                    }
                case BsonType.String:
                    {
                        if (TryGetStringAsSpan(out var temp))
                        {
                            if (decimal.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                            {
                                return ThrowHelper.InvalidTypeException(BsonType.Decimal, BsonType.String, Encoding.UTF8.GetString(temp));
                            }

                            return true;
                        }

                        return false;
                    }
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp))
                        {
                            value = new(temp);

                            return true;
                        }

                        return false;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp))
                        {
                            value = new(temp);

                            return true;
                        }

                        return false;
                    }
                case BsonType.Decimal:
                    return TryGetDecimal(out value);
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Decimal, type);
            }
        }
    }
}
