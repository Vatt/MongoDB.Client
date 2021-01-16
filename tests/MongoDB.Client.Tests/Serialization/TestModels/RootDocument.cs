using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class RootDocument : IEquatable<RootDocument>
    {
        [BsonId] public MongoDB.Client.Bson.Document.BsonObjectId Id { get; set; }
        public string TextFieldOne { get; set; }

        public string TextFieldTwo { get; set; }

        public string TextFieldThree { get; set; }

        public int IntField { get; set; }

        public double DoubleField { get; set; }

        public List<FirstLevelDocument> InnerDocuments { get; set; }

        public SomeEnum SomeEnumField { get; set; }

        public bool Equals(RootDocument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id)
                   && TextFieldOne == other.TextFieldOne
                   && TextFieldTwo == other.TextFieldTwo
                   && TextFieldThree == other.TextFieldThree
                   && IntField == other.IntField
                   && DoubleField.Equals(other.DoubleField)
                   && InnerDocuments.SequentialEquals(other.InnerDocuments)
                   && SomeEnumField == other.SomeEnumField;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RootDocument)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TextFieldOne, TextFieldTwo, TextFieldThree, IntField, DoubleField,
                InnerDocuments, (int)SomeEnumField);
        }
    }
}