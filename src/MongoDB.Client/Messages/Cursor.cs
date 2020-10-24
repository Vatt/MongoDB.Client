using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    class CursorOwner<T>
    {
        [BsonElementField(ElementName = "cursor")]
        public Cursor<T> Cursor { get; set; }

        [BsonElementField(ElementName = "ok")]
        public int Ok { get; set; }
    }

    [BsonSerializable]
    class Cursor<T>
    {
        [BsonElementField(ElementName = "id")]
        public int Id { get; set; }

        [BsonElementField(ElementName = "ns")]
        public int Namespace { get; set; }

        [BsonElementField(ElementName = "firstBatch")]
        public List<T> FirstBatch { get; set; }
    }
}
