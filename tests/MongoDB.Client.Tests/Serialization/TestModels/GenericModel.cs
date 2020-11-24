using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class NonGenericModel
    {
        public int A;
        public long B;
        public double C;
    }
    [BsonSerializable]
    public class GenericModel<T>
    {
        public T GenericValue;
        public List<T> GenericList;

    }
    [BsonSerializable]
    public record AdminIdentifier
    {
        public string Name;
    }
    [BsonSerializable]
    public record AdminDtoImpl : AdminIdentifier
    {

    }
    [BsonSerializable]
    public record AdminDtoArgs
    {
        public int arg1;
        public int arg2;
        public int arg3;
    }
    [BsonSerializable]
    public record AdminDtoArgsImpl : AdminDtoArgs
    {

    }
    [BsonSerializable]
    public record AdminDtoUpdateArgs
    {
        public int arg1;
        public int arg2;
        public int arg3;
    }
    [BsonSerializable]
    public record AdminDtoUpdateArgsImpl : AdminDtoUpdateArgs
    {

    }
    [BsonSerializable]
    public class BaseAdminRep<TA, TAD, TAA, TADA, TAUA, TAUD>
    {
        public TA Admin;
        public TAD AdminDto;
        public TAA AdminArgs;
        public TADA AdminDtoArgs;
        public TAUD AdminArgsUpdateDto;
    }
}