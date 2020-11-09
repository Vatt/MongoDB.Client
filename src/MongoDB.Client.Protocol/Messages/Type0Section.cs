namespace MongoDB.Client.Protocol.Messages
{
    public class Type0Section<T> : Section
    {
        public Type0Section(T item) : base(PayloadType.Type0)
        {
        }
    }
}