namespace MongoDB.Client.Exceptions
{
    public class MongoException : Exception
    {
        public MongoException(string? message) : base(message)
        {

        }

        public MongoException(string? message, Exception ex) : base(message, ex)
        {

        }
    }
}
