namespace MongoDB.Client.Exceptions
{
    internal class MongoExpressionException : MongoException
    {
        public MongoExpressionException(string? message) : base(message)
        {
        }
    }
}
