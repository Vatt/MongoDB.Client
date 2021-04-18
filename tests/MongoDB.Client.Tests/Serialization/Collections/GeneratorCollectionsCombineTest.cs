using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Collections
{
    [BsonSerializable]
    public partial class CollectionsCombineTestModel
    {
        public
            List<
                Dictionary<string,
                    ICollection<
                            IReadOnlyCollection<
                                IReadOnlyCollection<KeyValuePair<string,
                                    IReadOnlyList<
                                        IDictionary<string,
                                            IList<
                                                IReadOnlyDictionary<string, int>>>>>>>>>> Collections;
        public static CollectionsCombineTestModel Create()
        {
            return new()
            {
                Collections =
                new()
                {
                    new()
                    {
                        {
                            "42",
                            new List<IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, IReadOnlyList<IDictionary<string, IList<IReadOnlyDictionary<string, int>>>>>>>>()
                            {
                                new List<IReadOnlyCollection<KeyValuePair<string,IReadOnlyList<IDictionary<string,IList<IReadOnlyDictionary<string, int>>>>>>>()
                                {
                                    new Dictionary<string, IReadOnlyList<IDictionary<string,IList<IReadOnlyDictionary<string, int>>>>>()
                                    {
                                        {
                                            "42",
                                            new List<IDictionary<string,IList<IReadOnlyDictionary<string, int>>>>()
                                            {
                                                new Dictionary<string, IList<IReadOnlyDictionary<string, int>>>()
                                                {
                                                    {
                                                        "42",
                                                        new List<IReadOnlyDictionary<string, int>>()
                                                        {
                                                            new Dictionary<string, int>
                                                            {
                                                                {
                                                                    "42",
                                                                    42
                                                                }
                                                            }
                                                        }
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            };
        }
    }
    public class GeneratorCollectionsCombineTest : SerializationTestBase
    {
        [Fact]
        public async Task CollectionsCombineTest()
        {
            var model = CollectionsCombineTestModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
        }

    }
}
