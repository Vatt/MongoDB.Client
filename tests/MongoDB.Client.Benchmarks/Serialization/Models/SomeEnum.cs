using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonEnumSerializable(EnumRepresentation.Int32)]
    public enum SomeEnum
    {
        EnumValueOne = 0,
        EnumValueTwo = 1,
        EnumValueThree = 2
    }
}
