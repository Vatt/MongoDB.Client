using System;
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
    }
}
