using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    [Bson.Serialization.Attributes.BsonSerializable]
    class CursorOwner<T>
    {
        [Bson.Serialization.Attributes.BsonElementField(ElementName ="cursor")]
        public Cursor<T> Cursor { get; set; }

        [Bson.Serialization.Attributes.BsonElementField(ElementName = "ok")]
        public int Ok { get; set; }
    }

    [Bson.Serialization.Attributes.BsonSerializable]
    class Cursor<T>
    {
        [Bson.Serialization.Attributes.BsonElementField(ElementName = "id")]
        public int Id { get; set; }

        [Bson.Serialization.Attributes.BsonElementField(ElementName = "ns")]
        public int Namespace { get; set; }

        [Bson.Serialization.Attributes.BsonElementField(ElementName = "firstBatch")]
        public List<T> FirstBatch { get; set; }
    }
}
