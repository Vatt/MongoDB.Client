using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Attributes
{
    //TODO: in параметр со структурой выдает ошибку, в генераторе прописанно message.Id = BsonObjectId.NewObjectId()
    //[BsonSerializable]
    //public partial struct StructBsonIdModel : IEquatable<BsonIdModel>
    //{
    //    [BsonId]
    //    public BsonObjectId Id;
    //    public int SomeField;

    //    public override bool Equals(object obj)
    //    {
    //        return Equals((BsonIdModel)obj);
    //    }

    //    public bool Equals(BsonIdModel other)
    //    {
    //        return Id.Equals(other.Id) &&
    //               SomeField == other.SomeField;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Id, SomeField);
    //    }
    //}
    [BsonSerializable]
    public partial class BsonIdModel : IEquatable<BsonIdModel>
    {
        [BsonId]
        public BsonObjectId Id;
        public int SomeField;

        public override bool Equals(object obj)
        {
            return Equals(obj as BsonIdModel);
        }

        public bool Equals(BsonIdModel other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   SomeField == other.SomeField;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SomeField);
        }
    }
    public class GeneratorBsonIdTest : SerializationTestBase
    {
        [Fact]
        public async Task GenerateBsonIdTest()
        {
            var model = new BsonIdModel
            {
                SomeField = 42
            };
            var result = await RoundTripAsync(model);

            Assert.True(model.Id != default);
            Assert.Equal(model, result);
        }
        //[Fact]
        //public async Task GenerateStructWithBsonIdTest()
        //{
        //    var model = new StructBsonIdModel
        //    {
        //        SomeField = 42
        //    };
        //    var result = await RoundTripAsync(model);

        //    Assert.True(model.Id != default);
        //    Assert.Equal(model, result);
        //}
    }
}
