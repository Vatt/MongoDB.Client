using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class RootDocument : IIdentified
    {
        [BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public Bson.Document.BsonObjectId Id { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonIgnore]
        public ObjectId OldId { get; set; }

        public string TextFieldOne { get; set; }

        public string TextFieldTwo { get; set; }

        public string TextFieldThree { get; set; }

        public int IntField { get; set; }

        public double DoubleField { get; set; }

        public List<FirstLevelDocument> InnerDocuments { get; set; }

        public SomeEnum SomeEnumField { get; set; }
    }
}
