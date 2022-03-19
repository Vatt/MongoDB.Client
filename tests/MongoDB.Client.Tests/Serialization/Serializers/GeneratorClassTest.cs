using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Serializers
{
    [BsonSerializable]
    public partial class ClassWithPrimaryCtor : IEquatable<ClassWithPrimaryCtor>
    {
        public int A;
        public int B;
        public int C;
        public int D;
        public ClassWithPrimaryCtor(int a, int b, int c, int d)
        {
            A = d;
            B = b;
            C = c;
            D = d;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClassWithPrimaryCtor);
        }

        public bool Equals(ClassWithPrimaryCtor other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C &&
                   D == other.D;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }
    }
    [BsonSerializable]
    public partial class ClassWithManyCtors : IEquatable<ClassWithManyCtors>
    {
        public int A;
        public int B;
        public int C;
        public int D;
        [BsonConstructor]
        public ClassWithManyCtors(int a, int b, int c, int d)
        {
            A = d;
            B = b;
            C = c;
            D = d;
        }
        public ClassWithManyCtors(string a, string b, string c, string d)
        {
            A = int.Parse(d);
            B = int.Parse(b);
            C = int.Parse(c);
            D = int.Parse(d);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ClassWithManyCtors);
        }

        public bool Equals(ClassWithManyCtors other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C &&
                   D == other.D;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }
    }
    [BsonSerializable]
    public partial class ClassWithPrimaryCtorWithFreeField : IEquatable<ClassWithPrimaryCtorWithFreeField>
    {
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
        public ClassWithPrimaryCtorWithFreeField(int a, int b, int c, int d)
        {
            A = d;
            B = b;
            C = c;
            D = d;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClassWithPrimaryCtorWithFreeField);
        }

        public bool Equals(ClassWithPrimaryCtorWithFreeField other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C &&
                   D == other.D &&
                   E == other.E;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D, E);
        }
    }
    [BsonSerializable]
    public partial class ReadonlyClass : IEquatable<ReadonlyClass>
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;

        public ReadonlyClass(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ReadonlyClass);
        }

        public bool Equals(ReadonlyClass other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }
    }
    [BsonSerializable]
    public partial class ReadonlyClassWithFreeField : IEquatable<ReadonlyClassWithFreeField>
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        public int D;
        public ReadonlyClassWithFreeField(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ReadonlyClassWithFreeField);
        }

        public bool Equals(ReadonlyClassWithFreeField other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C &&
                   D == other.D;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }
    }
    [BsonSerializable]
    public partial class GetOnlyClass : IEquatable<GetOnlyClass>
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }

        public GetOnlyClass(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GetOnlyClass);
        }

        public bool Equals(GetOnlyClass other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }
    }
    [BsonSerializable]
    public partial class GetOnlyClassWithFreeField : IEquatable<GetOnlyClassWithFreeField>
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public int D { get; set; }

        public GetOnlyClassWithFreeField(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GetOnlyClassWithFreeField);
        }

        public bool Equals(GetOnlyClassWithFreeField other)
        {
            return other != null &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C &&
                   D == other.D;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }
    }

    public class GeneratorClassTest : SerializationTestBase
    {
        [Fact]
        public async Task ClassWithPrimaryCtorTest()
        {
            var model = new ClassWithPrimaryCtor(1, 2, 3, 4);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ClassWithManyCtorsTest()
        {
            var model = new ClassWithManyCtors(1, 2, 3, 4);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ClassWithPrimaryCtorWithFreeFieldTest()
        {
            var model = new ClassWithPrimaryCtorWithFreeField(1, 2, 3, 4);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyClassTest()
        {
            var model = new ReadonlyClass(1, 2, 3);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyClassWithFreeFieldTest()
        {
            var model = new ReadonlyClassWithFreeField(1, 2, 3);
            model.D = 42;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GetOnlyClassTest()
        {
            var model = new GetOnlyClass(1, 2, 3);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GetOnlyClassWithFreeFieldTest()
        {
            var model = new GetOnlyClassWithFreeField(1, 2, 3);
            model.D = 42;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }

    }
}
