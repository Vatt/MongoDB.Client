namespace MongoDB.Client.Exceptions
{
    public class MongoCommandException : MongoException
    {
        public int Code { get; }
        public string CodeName { get; }

        public MongoCommandException(string errorMessage, int code, string codeName) : base(errorMessage)
        {
            Code = code;
            CodeName = codeName;
        }
    }
}