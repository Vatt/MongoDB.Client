namespace MongoDB.Client.Exceptions
{
    public class MongoDBUriParserException : MongoException
    {
        public MongoDBUriParserException(string message) : base(message)
        {

        }
        public MongoDBUriParserException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
