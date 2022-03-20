using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial record TransactionTestModel(string A, string B, string C, int D, int E);
    public class ClientTransactionTest : ClientTestBase
    {
        [Fact]
        public async Task TransactionSimpleTest()
        {
            var item = new TransactionTestModel("TransactionTestModelA", "TransactionTestModelB", "TransactionTestModelC", 42, 42);
            var items = new TransactionTestModel[1024];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = item;
            }
        }
    }
}
