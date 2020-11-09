﻿using System;
using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        public bool TrySkip(int bsonType)
        {
            switch (bsonType)
            {
                case 1:
                {
                    _input.Advance(sizeof(double));
                    return true;
                }
                case 2:
                {
                    if (!TryGetInt32(out var length))
                    {
                        return false;
                    }
                    _input.Advance(length);
                    return true;
                }
                case 3:
                {
                    
                    if (!TryGetInt32(out var docLength))
                    {
                        return false;
                    }
                    _input.Advance(docLength - sizeof(int));
                    return true;
                }
                case 4:
                {
                    if (!TryGetInt32(out var arrayLength)) 
                    {
                        return false;
                    }

                    _input.Advance(arrayLength - sizeof(int));
                    return true;
                }
                case 5:
                {
                    if (!TryGetInt32(out var binDataLength))
                    {
                        return false;
                    }

                    _input.Advance(binDataLength - sizeof(int));
                    return true;
                }
                case 7:
                {
                    _input.Advance(12);
                    return true;
                }
                case 8:
                {
                    _input.Advance(1);
                    return true;
                }
                case 9:
                {
                    _input.Advance(sizeof(long));
                    return true;
                }
                case 10:
                {
                    return true;
                }
                case 16:
                {
                    _input.Advance(sizeof(int));
                    return true;
                }
                case 17:
                {
                    _input.Advance(sizeof(long));
                    return true;
                }
                case 18:
                {
                    _input.Advance(sizeof(long));
                    return true;
                }
                default:
                {
                    return ThrowHelper.UnknownTypeException<bool>(bsonType);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetGuidWithBsonType(int bsonType, out Guid value)
        {
            value = default;
            
            if (bsonType == 5)
            {
                return TryGetBinaryDataGuid(out value);
            }
            if (bsonType == 2)
            {
                return TryGetGuidFromString(out value);                
            }
            throw new ArgumentException("Unsupported Guid type");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDateTimeWithBsonType(int bsonType, out DateTimeOffset value)
        {
            value = default;

            if (bsonType == 3)
            {
                return TryGetDatetimeFromDocument(out value);
            }
            if (bsonType == 9)
            {
                return TryGetUtcDatetime(out value);
            }
            if (bsonType == 18)
            {
                return TryGetUtcDatetime(out value);
            }
            throw new ArgumentException("Unsupported DateTime type");
        }
    }
}
