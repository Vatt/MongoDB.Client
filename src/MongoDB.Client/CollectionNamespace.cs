namespace MongoDB.Client
{
    public class CollectionNamespace
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly string _fullName;

        public CollectionNamespace(string databaseName, string collectionName)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _fullName = _databaseName + "." + _collectionName;
        }

        public string DatabaseName => _databaseName;

        public string CollectionName => _collectionName;

        public string FullName => _fullName;
    }
}
