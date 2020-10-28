namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        internal MongoCollection(MongoDatabase database, string name)
        {
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }

        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }
    }
}
