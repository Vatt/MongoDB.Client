namespace MongoDB.Client.Messages
{
    public class QueryResult<T> : IParserResult
    {
        public QueryResult(T result)
        {
            Result = result;
        }

        public T Result { get; } 
    }
}