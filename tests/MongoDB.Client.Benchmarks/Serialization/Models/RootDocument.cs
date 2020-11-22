using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public class RootDocument
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [Bson.Serialization.Attributes.BsonIgnore]
        public ObjectId OldId { get; set; }
        
        [Bson.Serialization.Attributes.BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public MongoDB.Client.Bson.Document.BsonObjectId Id { get; set; }
        public string TextFieldOne { get; set; }

        public string TextFieldTwo { get; set; }

        public string TextFieldThree { get; set; }

        public int IntField { get; set; }

        public double DoubleField { get; set; }

        public List<FirstLevelDocument> InnerDocuments { get; set; }

        public SomeEnum SomeEnumField { get; set; }
    }
}
