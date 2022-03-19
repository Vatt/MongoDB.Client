namespace MongoDB.Client.Bson.Serialization.Exceptions
{

    public class SerializerNotFoundException : Exception
    {
        public SerializerNotFoundException(string serializer) : base($"{serializer}: not found)")
        {

        }
    }
}