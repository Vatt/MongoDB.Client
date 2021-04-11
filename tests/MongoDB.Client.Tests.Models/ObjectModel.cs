using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class ObjectModel : IEquatable<ObjectModel>

    {
        public object ObjectProp0 { get; }
        public object ObjectProp1 { get; }

        public ObjectModel(object objectProp0, object objectProp1)
        {
            ObjectProp0 = objectProp0;
            ObjectProp1 = objectProp1;
        }

        public bool Equals(ObjectModel? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ObjectProp0.Equals(other.ObjectProp0) && ObjectProp1.Equals(other.ObjectProp1);
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

            return Equals((ObjectModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ObjectProp0, ObjectProp1);
        }
    }
}
