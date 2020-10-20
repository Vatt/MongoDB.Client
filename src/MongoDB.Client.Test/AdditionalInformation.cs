using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    [BsonSerializable]
    class Information
    {
        [BsonElementField(ElementName = "_id")]
        public Guid TypeId;
    }

    [BsonSerializable]
    class AdditionalInformation
    {
        [BsonElementField]
        public Guid TypeId;
        [BsonElementField]
        public List<Information> Informations;
    }
}
