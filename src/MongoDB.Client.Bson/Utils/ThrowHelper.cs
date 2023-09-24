using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MongoDB.Client.Bson.Utils
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T MissedDocumentEndMarkerException<T>()
        {
            throw new NotSupportedException($"Document end marker was not found");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnknownSubtypeException<T>(int subtype)
        {
            throw new NotSupportedException($"Unknown document subtype: " + subtype.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnknownTypeException<T>(int type)
        {
            throw new NotSupportedException($"Unknown document type: " + type.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedGuidTypeException<T>(int bsonType)
        {
            throw new NotSupportedException("Unsupported Guid type: " + bsonType.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedDecimalTypeException<T>(int bsonType)
        {
            throw new NotSupportedException("Unsupported Decimal type: " + bsonType.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedStringDecimalException<T>(string value)
        {
            throw new NotSupportedException("Unsupported Decimal string: " + value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T UnsupportedDateTimeTypeException<T>(int bsonType)
        {
            throw new NotSupportedException("Unsupported DateTime type: " + bsonType.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static T NotImplementedException<T>(string value)
        {
            throw new NotImplementedException($"'{value}' not implemented");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ObjectIdParseException()
        {
            throw new ArgumentException($"The array must be larger than 12 bytes");
        }
    }
}
