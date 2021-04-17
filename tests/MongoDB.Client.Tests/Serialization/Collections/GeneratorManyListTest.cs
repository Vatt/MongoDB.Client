using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;
namespace MongoDB.Client.Tests.Serialization
{
    [BsonSerializable]
    public partial class ManyListModel
    {
        public string Name;
        public List<List<List<List<List<List<long>>>>>> Longs;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return obj is ManyListModel other && Equals(Longs, other.Longs);
        }

        public static bool Equals(List<List<List<List<List<List<long>>>>>> list0, List<List<List<List<List<List<long>>>>>> list1)
        {
            if (list0 is null)
            {
                throw new ArgumentNullException(nameof(list0));
            }
            if (list1 is null)
            {
                throw new ArgumentNullException(nameof(list0));
            }

            for (int i0 = 0; i0 < list0.Count; i0++)
            {
                for (int i1 = 0; i1 < list0.Count; i1++)
                {
                    for (int i2 = 0; i2 < list0.Count; i2++)
                    {
                        for (int i3 = 0; i3 < list0.Count; i3++)
                        {
                            for (int i4 = 0; i4 < list0.Count; i4++)
                            {
                                for (int i5 = 0; i5 < list0.Count; i5++)
                                {
                                    if (!list0[i0][i1][i2][i3][i4][i5].Equals(list1[i0][i1][i2][i3][i4][i5]))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Longs);
        }
        public static ManyListModel Create()
        {
            return new ManyListModel()
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
        }
    }
    public class GeneratorManyListTest : SerializationTestBase
    {
        [Fact]
        public async Task ManyListTest()
        {
            var model = ManyListModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
