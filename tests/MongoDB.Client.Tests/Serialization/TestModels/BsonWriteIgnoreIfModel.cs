using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class BsonWriteIgnoreIfModel
    {
        public int Field { get; set; }

        [BsonWriteIgnoreIf("Field==42")]
        public string IgnoredField0{ get; set; }

        [BsonWriteIgnoreIf(@"IgnoredField0.Equals(""lol0"")")]
        public string IgnoredField1 { get; set; }
    }
}
