﻿using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class DeleteResult : IParserResult
    {
        [BsonElementField(ElementName = "n")]
        public int N { get; set; }
        
        [BsonElementField(ElementName = "ok")]
        public double Ok { get; set; }
    }
}