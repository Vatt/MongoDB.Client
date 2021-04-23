using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Serializers
{
    
    [BsonSerializable]
    public partial class RootTestModel : IEquatable<RootTestModel>
    {
        [BsonSerializable]
        public partial struct InnerStructTestModel : IEquatable<InnerStructTestModel>
        {
            public int A;

            public override bool Equals(object obj)
            {
                return obj is InnerStructTestModel model && Equals(model);
            }

            public bool Equals(InnerStructTestModel other)
            {
                return A == other.A;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A);
            }
        }
        [BsonSerializable]
        public partial class InnerClassTestModel : IEquatable<InnerClassTestModel>
        {
            public int B;

            public override bool Equals(object obj)
            {
                return Equals(obj as InnerClassTestModel);
            }

            public bool Equals(InnerClassTestModel other)
            {
                return other != null &&
                       B == other.B;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(B);
            }
        }
        [BsonSerializable]
        public partial record InnerRecordTestModel(int C);

        public RootTestModel(InnerStructTestModel _struct, InnerClassTestModel _class, InnerRecordTestModel _record)
        {
            Struct = _struct;
            Class = _class;
            Record = _record;
        }
        public InnerStructTestModel Struct { get; }
        public InnerClassTestModel Class { get; }
        public InnerRecordTestModel Record { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as RootTestModel);
        }

        public bool Equals(RootTestModel other)
        {
            return other != null &&
                   Struct.Equals(other.Struct) &&
                   EqualityComparer<InnerClassTestModel>.Default.Equals(Class, other.Class) &&
                   EqualityComparer<InnerRecordTestModel>.Default.Equals(Record, other.Record);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Struct, Class, Record);
        }

        public static RootTestModel Create() => new RootTestModel(new() { A = 1 }, new() { B = 2 }, new(3));
    }
    public class GeneratorInnerSerializersTest:SerializationTestBase
    {
        [Fact]
        public async Task GeneratorInnerClassStructRecordTest()
        {
            var model = RootTestModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
