using MongoDB.Client.Tests.Serialization.TestModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedManyListTest : BaseSerialization
    {
        [Fact]
        public async Task ManyListTest()
        {
            var model = new ManyListModel()
            {
                Name = "ManyListTest",
                Longs = new List<List<List<List<List<List<long>>>>>>()
                {
                    new List<List<List<List<List<long>>>>>()
                    {
                        new List<List<List<List<long>>>>()
                        {
                            new List<List<List<long>>>()
                            {
                                new List<List<long>>()
                                {
                                    new List<long> {1, 2, 3, 4, 5},
                                    new List<long> {5, 6, 7, 8, 9},
                                    new List<long> {10, 11, 12, 13, 14}
                                },
                                new List<List<long>>()
                                {
                                    new List<long> {11, 12, 13, 14, 15},
                                    new List<long> {15, 16, 17, 18, 19},
                                    new List<long> {110, 111, 112, 113, 114}
                                }

                            },
                            new List<List<List<long>>>()
                            {
                                new List<List<long>>()
                                {
                                    new List<long> {21, 22, 23, 24,25},
                                    new List<long> {25, 26, 27, 28, 29},
                                    new List<long> {210, 211, 212, 213, 214}
                                },
                                new List<List<long>>()
                                {
                                    new List<long> {211, 212, 213, 214, 215},
                                    new List<long> {215, 216, 217, 218, 219},
                                    new List<long> {2110, 2111, 2112, 2113, 2114}
                                }
                            }
                        },
                        new List<List<List<List<long>>>>()
                        {
                            new List<List<List<long>>>()
                            {
                                new List<List<long>>()
                                {
                                    new List<long> {31, 32, 33, 34, 35},
                                    new List<long> {35, 36, 37, 38, 39},
                                    new List<long> {310, 311, 312, 313, 314}
                                },
                                new List<List<long>>()
                                {
                                    new List<long> {311, 312, 313, 314, 315},
                                    new List<long> {315, 316, 317, 318, 319},
                                    new List<long> {3110, 3111, 3112, 3113, 3114}
                                }

                            },
                            new List<List<List<long>>>()
                            {
                                new List<List<long>>()
                                {
                                    new List<long> {321, 322, 323, 324, 325},
                                    new List<long> {325, 326, 327, 328, 329},
                                    new List<long> {3210, 3211, 3212, 3213, 3214}
                                },
                                new List<List<long>>()
                                {
                                    new List<long> {3211, 3212, 3213, 3214, 3215},
                                    new List<long> {3215, 3216, 3217, 3218, 3219},
                                    new List<long> {32110, 32111, 32112, 32113, 32114}
                                }
                            }
                        }
                    }
                }
            };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}