using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Tests.Models
{
    public class ConstrcutorOnlyModelBase
    {
        public int D { get; }
        public int E { get; }

        public ConstrcutorOnlyModelBase(int D, int E)
        {
            this.D = D;
            this.E = E;
        }
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters | GeneratorMode.IfConditions)]
    public partial class ConstrcutorOnlyModel : ConstrcutorOnlyModelBase, IEquatable<ConstrcutorOnlyModel>
    {
        public SomeEnum? SomeEnum { get; }
        public int A { get; }
        public int? B { get; }
        public List<int>? C { get; }

        public ConstrcutorOnlyModel(SomeEnum? SomeEnum, int A, int? B, int D, int E) : base(D, E)
        {
            this.SomeEnum = SomeEnum;
            this.A = A;
            this.B = B;
            C = null;
        }

        public bool Equals(ConstrcutorOnlyModel? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SomeEnum == other.SomeEnum && A == other.A && B == other.B && Equals(C, other.C);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ConstrcutorOnlyModel) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SomeEnum, A, B, C);
        }
    }
}
