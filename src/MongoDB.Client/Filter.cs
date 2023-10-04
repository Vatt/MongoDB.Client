using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Filters;

namespace MongoDB.Client
{
    public abstract class Filter : IBsonSerializer<Filter>
    {
        public static Filter Eq<T, TValue>(Expression<Func<T, TValue>> expr, TValue value)
            where T : IBsonSerializer<T>
        {
            var propertyName = ExpressionHelper.GetPropertyName(expr);

            return new EqFilter<TValue>(propertyName, value);
        }
        protected abstract void Write(ref BsonWriter writer);
        public static Filter Document(BsonDocument document) => new BsonDocumentFilter(document);
        public static void WriteBson(ref BsonWriter writer, in Filter message)
        {
            message.Write(ref writer);
        }

        public static bool TryParseBson(ref BsonReader reader, out Filter message) => throw new NotImplementedException();
    }
}
