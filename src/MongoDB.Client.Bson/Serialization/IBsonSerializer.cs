namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializer
    {
        void Write(object message);
    }
}
