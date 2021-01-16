using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.ConsoleApp.Models
{
    [BsonSerializable]
    public partial class RootDocument
    {
        [BsonId]
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
