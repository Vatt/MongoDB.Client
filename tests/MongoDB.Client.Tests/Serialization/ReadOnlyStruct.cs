using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class ReadOnlyStruct : BaseSerialization
    {
        [Fact]
        public async Task ReadOnlyStructTest()
        {
            var model = new ReadonlyStruct(42, 42, "42");
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}
