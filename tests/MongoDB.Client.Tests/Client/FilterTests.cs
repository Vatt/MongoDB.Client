using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial record FilterTestRecord(int IntProp, string StringProp, double DoubleProp, long LongProp, decimal DecimalProp);

    class Wrapper
    {
        public WrappedArray WrappedArrayField = new();
        public WrappedArray WrappedArrayProperty { get; } = new();
        public WrappedInt32 WrappedInt32 { get; } = new();
    }
    class WrappedArray
    {
        public int[] FieldArray = new[] { 1, 2, 3, 4, 5 };
        public int[] PropertyArray { get; } = new[] { 5, 4, 3, 2, 1 };
    }
    class WrappedInt32
    {
        public int Value { get; } = 1;
    }

    public class FilterTests : ClientTestBase
    {

        private static int[] arr = new int[] { 1, 2, 3 };
        private static bool boolVar = false;
        private static Wrapper wrapper { get; set; } = new();

        [Fact]
        public async Task TestFilters()
        {
            var client = await CreateStandaloneClient(1);
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FilterTestRecord>("FilterTestCollection" + Guid.NewGuid());
            Memory<FilterTestRecord> data = new[]
            {
                new FilterTestRecord(1, "1", 1, 1, 1),
                new FilterTestRecord(2, "2", 2, 2, 2),
                new FilterTestRecord(3, "3", 3, 3, 3),
                new FilterTestRecord(4, "4", 4, 4, 4)
            };

            await InsertAsync(data.ToArray(), collection);
            
            var result1 = await collection.Find(x => x.IntProp == 1).SingleOrDefaultAsync();
            var result2 = await collection.Find(x => x.StringProp == "2").SingleOrDefaultAsync();
            var result3 = await collection.Find(x => x.DoubleProp == 3).SingleOrDefaultAsync();
            var result4 = await collection.Find(x => x.DecimalProp == 4).SingleOrDefaultAsync();
            var result5 = await collection.Find(x => wrapper.WrappedInt32.Value == x.IntProp).SingleOrDefaultAsync();
            var result6 = await collection.Find(x => x.IntProp == wrapper.WrappedInt32.Value).SingleOrDefaultAsync();
            var result7 = await collection.Find(x => x.IntProp < wrapper.WrappedInt32.Value).SingleOrDefaultAsync();
            var result8 = await collection.Find(x => x.IntProp > wrapper.WrappedInt32.Value).ToListAsync();
            var result9 = await collection.Find(x => x.IntProp >= wrapper.WrappedInt32.Value).ToListAsync();
            var result10 = await collection.Find(x => x.IntProp <= wrapper.WrappedInt32.Value).ToListAsync();
            var result11 = await collection.Find(x => arr.Contains(x.IntProp)).ToListAsync();
            var result12 = await collection.Find(x => arr.Contains(x.IntProp) == false).ToListAsync();
            var result13 = await collection.Find(x => arr.Contains(x.IntProp) != boolVar).ToListAsync();
            var result14 = await collection.Find(x => x.IntProp == 1 || x.IntProp == 2).ToListAsync();
            var result15 = await collection.Find(x => x.IntProp == 1 && x.StringProp == "2").ToListAsync();
            var result16 = await collection.Find(x => (x.IntProp == 1 && x.StringProp == "1") || (x.IntProp == 2 && x.StringProp == "2")).ToListAsync();
            Assert.Equal(1, result1.IntProp);
            Assert.Equal("2", result2.StringProp);
            Assert.Equal(3, result3.DoubleProp);
            Assert.Equal(4, result4.DecimalProp);
            Assert.Equal(1, result6.IntProp);
            Assert.Null(result7);
            Assert.Equal(3, result8.Count);
            Assert.Equal(4, result9.Count);
            Assert.Single(result10);
            Assert.Equal(3, result11.Count);
            Assert.Single(result12);
            Assert.Equal(3, result13.Count);
            Assert.Equal(2, result14.Count);
            Assert.Equal(1, result15.Count);
            Assert.Equal(2, result15.Count);
            await collection.DropAsync();
        }
    }
}
