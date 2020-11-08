namespace MongoDB.Client.Protocol.Messages
{
    public class Section
    {
        public Section(PayloadType type)
        {
            Type = type;
        }

        public PayloadType Type { get;}
    }
}