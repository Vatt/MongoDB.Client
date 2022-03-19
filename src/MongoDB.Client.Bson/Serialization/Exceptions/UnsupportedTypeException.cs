namespace MongoDB.Client.Bson.Serialization.Exceptions
{
    public class UnsupportedTypeException : Exception
    {
        public UnsupportedTypeException(string typeName) : base($"Unsupported type: {typeName}")
        {

        }
    }
}
