using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    [BsonSerializable]
    public partial class BsonWriteIgnoreIfTestModel
    {
        public int Field { get; set; }

        [BsonWriteIgnoreIf("Field==42")]
        public string IgnoredField0 { get; set; }

        [BsonWriteIgnoreIf("Field==42")]
        public List<int> ListValue { get; set; }

        [BsonWriteIgnoreIf(@"IgnoredField0.Equals(""lol0"")")]
        public string IgnoredField1
        {
            get; set;
        }
    }
    public class GeneratorWriteIgnoreIfTest : SerializationTestBase
    {
        [Fact]
        public async Task WriteIgnoreIfTest()
        {
            var doc = new BsonWriteIgnoreIfTestModel
            {
                Field = 42,
                IgnoredField0 = "lol0",
                IgnoredField1 = "lol1",
                ListValue = new List<int> { 1, 2, 3 },

            };
            var result = await RoundTripAsync(doc);
            Assert.Equal(doc.Field, result.Field);
            Assert.Null(result.IgnoredField0);
            Assert.Null(result.ListValue);
            doc = new BsonWriteIgnoreIfTestModel
            {
                Field = 41,
                IgnoredField0 = "lol0",
                IgnoredField1 = "lol1",
                ListValue = new List<int> { 1, 2, 3 },

            };
            result = await RoundTripAsync(doc);
            Assert.Equal(doc.Field, result.Field);
            Assert.Equal(doc.ListValue, result.ListValue);
            Assert.Null(result.IgnoredField1);

        }
    }
}
