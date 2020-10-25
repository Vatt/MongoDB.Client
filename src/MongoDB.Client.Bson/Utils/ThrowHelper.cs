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
    }
}
