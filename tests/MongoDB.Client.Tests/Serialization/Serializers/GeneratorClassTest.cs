﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ClassWithPrimaryCtor(int a , int b, int c, int d)
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
        [BsonConstructor]
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
        [BsonConstructor]
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
        [BsonConstructor]
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

    public class GeneratorClasssTest : SerializationTestBase
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
