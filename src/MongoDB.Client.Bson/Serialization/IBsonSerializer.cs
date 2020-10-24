namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializer
    {
        //bool TryParse(ref MongoDBBsonReader reader, [MaybeNullWhen(false)] out object message);

        void Write(object message);
    }
}
