using System;

namespace MongoDB.Client.Bson.Serialization.Exceptions
{
    public class SerializerEndMarkerException : Exception
    {
        public SerializerEndMarkerException(string serializer, byte endMarker)
            : base($"{serializer}: end marker bson document mismatch (endMarker: {endMarker})")
        {

        }
    }
}