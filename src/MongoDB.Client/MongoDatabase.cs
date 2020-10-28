namespace MongoDB.Client
{
    public class MongoDatabase
    {
        public MongoClient Client { get; }
        public string Name { get; }

        internal MongoDatabase(MongoClient client, string name)
        {
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            return new MongoCollection<T>(this, name);
        }
    }
}
