using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class NullableTest : BaseSerialization
    {
        [Fact]
        public async Task  Nullable()
        {
            var nullableModel = NullableModel.Create();
            var result = await RoundTripAsync(nullableModel);
        }
    }
}
