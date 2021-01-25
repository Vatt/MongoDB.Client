namespace MongoDB.Client.Exceptions
{
    public class MongoDropCollectionException : MongoException
    {
        public MongoDropCollectionException(string error) : base(error)
        {

        }
    }
}