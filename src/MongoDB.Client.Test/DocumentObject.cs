//using MongoDB.Client.Bson.Document;
//using MongoDB.Client.Bson.Reader;
//using MongoDB.Client.Bson.Serialization;
//using MongoDB.Client.Bson.Serialization.Attributes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MongoDB.Client.Test
//{
//    [BsonSerializable]
//    class DocumentObject
//    {
//        [BsonElementField(ElementName = "_id")]
//        public Guid Id { get; set; }

//        public Guid TypeId { get; set; }


//        public DateTimeOffset LastModifiedDate { get; set; }
//        //public BsonDocument LastModifiedDate { get; set; }

//        public DateTimeOffset CreatedDate { get; set; }
//        //public BsonDocument CreatedDate { get; set; }

//        public bool Deleted { get; set; }

//        public string MetaInformation;
//        public List<BsonDocument> AdditionalInformation { get; set; }
//    }

//}
