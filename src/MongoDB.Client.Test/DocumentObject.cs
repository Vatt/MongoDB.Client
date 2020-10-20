using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{

    [BsonSerializable]
    class DocumentObject
    {
        [BsonElementField(ElementName = "_id")]
        public Guid Id;

        [BsonElementField]
        public Guid TypeId;

        [BsonElementField]
        public DateObject LastModifiedDate;

        [BsonElementField]
        public DateObject CreatedDate;

        [BsonElementField]
        public object MetaInformation;

        [BsonElementField]
        public bool Deleted;

        [BsonElementField]
        public List<AdditionalInformation> AdditionalInformation;
    }

}
