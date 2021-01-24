using MongoDB.Client.Test.Models;
using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientCommonTest : ClientTestBase
    {
        [Fact]
        async Task CommontTest()
        {
            var model = CommonModel.Create();
            var result = await InsertFindDeleteAsync(model);
        }
        [Fact]
        async Task CustomTest()
        {
            var model = CustomModel.Create();
            var result = await InsertFindDeleteAsync(model);
        }
    }
}
