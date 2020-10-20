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
    class DateObject
    {
        [BsonElementField]
        public DateTimeOffset DateTime;

        [BsonElementField]
        public long Ticks;

        [BsonElementField]
        public int Offset;

    }

}


