using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedGenericTest : BaseSerialization
    {
        [Fact]
        public async Task GenericTest()
        {
            var simpleModel = new GenericModel<long>()
            {
                GenericValue = long.MaxValue,
                GenericList = new System.Collections.Generic.List<long>() { 1, 2, 3, 4, 5 },
            };
            var result = await RoundTripAsync<GenericModel<long>>(simpleModel, GenericModel<long>.Serializer);

            var nongeneric = new NonGenericModel()
            {
                A = 24,
                B = 24,
                C = 24,
            };
            var docgeneric = new GenericModel<NonGenericModel>()
            {
                GenericValue = new NonGenericModel
                {
                    A = 42,
                    B = 42,
                    C = 42,
                },
                GenericList = new System.Collections.Generic.List<NonGenericModel>() { nongeneric, null, nongeneric, nongeneric },
            };
            var docresult = await RoundTripAsync<GenericModel<NonGenericModel>>(docgeneric, GenericModel<NonGenericModel>.Serializer);
        }
    }
}