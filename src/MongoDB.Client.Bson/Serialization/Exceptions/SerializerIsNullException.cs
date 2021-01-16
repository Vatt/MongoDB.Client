using System;

namespace MongoDB.Client.Bson.Serialization.Exceptions
{
    public class SerializerIsNullException : Exception
    {
        public SerializerIsNullException(string serializer) : base($"{serializer}: is null)")
        {

        }
    }
}