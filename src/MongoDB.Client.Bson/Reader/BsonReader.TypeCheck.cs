using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
    	public bool TryGet(BsonType type, out double value)
    	{
            switch(type)
            {
                case BsonType.Double:
                    return TryGetDouble(out value);
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp))
                        {
                            value = temp;
                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp))
                        {
                            value = temp;
                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.String:
                    {
                        if (TryGetStringAsSpan(out var temp))
                        {
                            if (double.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                            {
                                return ThrowHelper.InvalidTypeException(BsonType.Double, BsonType.String, Encoding.UTF8.GetString(temp));
                            }

                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.Null:
                    value = default;
                    return true;
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Double, type);
            }
    	}
        public bool TryGet(BsonType type, out double? value)
        {
            if (TryGet(type, out double temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out string? value)
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
        public bool TryGet(BsonType type, out BsonDocument? value)
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
        public bool TryGet(BsonType type, out BsonArray? value)
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
            return ThrowHelper.InvalidTypeException(BsonType.Array, type);
        }
        public bool TryGet(BsonType type, out BsonBinaryData value)
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
        public bool TryGet(BsonType type, out BsonBinaryData? value)
        {
            if (TryGet(type, out BsonBinaryData temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out BsonObjectId value)
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
        public bool TryGet(BsonType type, out BsonObjectId? value)
        {
            if (TryGet(type, out BsonObjectId temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out bool value)
        {
            switch (type)
            {
                case BsonType.Boolean:
                    return TryGetBoolean(out value);
                case BsonType.Double:
                    {
                        if (TryGetDouble(out double temp))
                        {
                            value = temp is not 0.0;
                            return true;

                        }

                        value = default;
                        return false;
                    }
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp))
                        {
                            value = temp is not 0;
                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp))
                        {
                            value = temp is not 0;
                            return true;
                        }

                        value = default;
                        return false;
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
        public bool TryGet(BsonType type, out bool? value)
        {
            if (TryGet(type, out bool temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out DateTimeOffset value)
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
                        if (TryGetStringAsSpan(out var temp))
                        {
                            if (DateTimeOffset.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                            {
                                return ThrowHelper.InvalidTypeException(BsonType.UtcDateTime, BsonType.String, Encoding.UTF8.GetString(temp));
                            }

                            return true;

                        }

                        value = default;
                        return false;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.UtcDateTime, type);
            }
        }
        public bool TryGet(BsonType type, out DateTimeOffset? value)
        {
            if (TryGet(type, out DateTimeOffset temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryGet(BsonType type, out int value)
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
                        if (TryGetCStringAsSpan(out var temp))
                        {
                            if (int.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                            {
                                return ThrowHelper.InvalidTypeException(BsonType.Int32, BsonType.String, Encoding.UTF8.GetString(temp));
                            }

                            return true;
                        }

                        value = default;
                        return false;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Int32, type);
            }
        }
        public bool TryGet(BsonType type, out int? value)
        {
            if (TryGet(type, out int temp))
            {
                value = temp;
                return true;

            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out BsonTimestamp value)
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
        public bool TryGet(BsonType type, out BsonTimestamp? value)
        {
            if (TryGet(type, out BsonTimestamp temp))
            {
                value = temp;
                return true;

            }

            value = default;
            return false;
        }
        public bool TryGet(BsonType type, out long value)
        {
            switch (type)
            {
                case BsonType.Int64:
                    return TryGetInt64(out value);
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp))
                        {
                            value = temp;
                            return true;

                        }

                        value = default;
                        return false;
                    }
                case BsonType.Null:
                    value = default;
                    return true;
                case BsonType.String:
                    {
                        if (TryGetCStringAsSpan(out var temp))
                        {
                            if (long.TryParse(MemoryMarshal.Cast<byte, char>(temp), CultureInfo.InvariantCulture, out value) is false)
                            {
                                return ThrowHelper.InvalidTypeException(BsonType.Int32, BsonType.String, Encoding.UTF8.GetString(temp));
                            }

                            return true;
                        }

                        value = default;
                        return false;
                    }
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Int32, type);
            }
        }
        public bool TryGet(BsonType type, out long? value)
        {
            if (TryGet(type, out long temp) is false)
            {
                value = default;
                return false;
            }

            value = temp;
            return true;
        }
        public bool TryGet(BsonType type, out decimal value)
        {
            switch (type)
            {
                case BsonType.Double:
                    {
                        if (TryGetDouble(out double doubleValue))
                        {
                            value = new(doubleValue);
                            return true;
                        }

                        value = default;
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

                        value = default;
                        return false;
                    }
                case BsonType.Int32:
                    {
                        if (TryGetInt32(out int temp))
                        {
                            value = new(temp);
                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.Int64:
                    {
                        if (TryGetInt64(out long temp))
                        {
                            value = new(temp);
                            return true;
                        }

                        value = default;
                        return false;
                    }
                case BsonType.Decimal:
                    return TryGetDecimal(out value);
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.Decimal, type);
            }
        }
        public bool TryGet(BsonType type, out decimal? value)
        {
            if (TryGet(type, out decimal temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGet(BsonType type, out Guid value)
        {
            switch (type)
            {
                case BsonType.BinaryData:
                    return TryGetBinaryDataGuid(out value);
                case BsonType.String:
                    return TryGetGuidFromString(out value);
                default:
                    value = default;
                    return ThrowHelper.InvalidTypeException(BsonType.BinaryData, type);
            }
        }
        public bool TryGet(BsonType type, out Guid? value)
        {
            if (TryGet(type, out Guid temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
    }
}
