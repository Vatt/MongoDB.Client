﻿using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class ConstrcutorOnlyTest : SerializationTestBase
    {
        [Fact]
        public async Task ConstructorOnly()
        {
            var model = new ConstrcutorOnlyModel(SomeEnum.EnumValueOne, 42, null, 42, 42);
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}
