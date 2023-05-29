using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    public class ConstrcutorOnlyModelBase
    {
        public int D { get; }
        public int E { get; }

        public ConstrcutorOnlyModelBase(int dd, int ee)
        {
            this.D = dd;
            E = ee;
        }
    }
    [BsonSerializable(GeneratorMode.ConstructorOnlyParameters | GeneratorMode.IfConditions)]
    public partial class ConstrcutorOnlyModel : ConstrcutorOnlyModelBase, IEquatable<ConstrcutorOnlyModel>
    {
        public SomeEnum? SomeEnum { get; }
        public int A { get; }
        public int? B { get; }
        public List<int>? C { get; }

        public ConstrcutorOnlyModel(SomeEnum? someEnum, int a, int? b, int d, int e) : base(d, e)
        {
            SomeEnum = someEnum;
            A = a;
            B = b;
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

            return Equals((ConstrcutorOnlyModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SomeEnum, A, B, C);
        }
    }
}
