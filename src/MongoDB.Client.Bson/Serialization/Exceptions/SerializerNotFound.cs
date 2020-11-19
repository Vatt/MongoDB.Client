using System;

namespace MongoDB.Client.Bson.Serialization.Exceptions
{
    
    public class SerializerNotFound : Exception
    {
        public SerializerNotFound(string serializer) : base($"{serializer}: not found)")
        {
            
        }
    }
}