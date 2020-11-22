﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedNullCheckTest : BaseSerialization
    {
        [Fact]
        public async Task NullCheckTest()
        {
            var model = new NullCheckModel(null, new List<NullCheckData>()
                {
                    new NullCheckData(42),
                    null,
                    new NullCheckData(42),
                });
            SerializersMap.TryGetSerializer<NullCheckModel>(out var serializer);
            var result = await RoundTripAsync<NullCheckModel>(model, serializer);

            Assert.Equal(result, model);
        } 
    }
}