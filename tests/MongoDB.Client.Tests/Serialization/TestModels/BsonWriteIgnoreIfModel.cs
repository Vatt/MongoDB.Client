using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class BsonWriteIgnoreIfModel
    {
        public int Field { get; set; }

        [BsonWriteIgnoreIf("Field==42")]
        public string IgnoredField0 { get; set; }

        [BsonWriteIgnoreIf("Field==42")]
        public List<int> ListValue { get; set; }

        [BsonWriteIgnoreIf(@"IgnoredField0.Equals(""lol0"")")]
        public string IgnoredField1 { get; set; }


    }
}
