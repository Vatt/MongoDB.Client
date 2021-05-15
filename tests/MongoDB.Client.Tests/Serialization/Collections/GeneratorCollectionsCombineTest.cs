using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;
using CombinedType = System.Collections.Generic.List<
                        System.Collections.Generic.Dictionary<string,
                            System.Collections.Generic.ICollection<
                                    System.Collections.Generic.IReadOnlyCollection<
                                        System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<string,
                                            System.Collections.Generic.IReadOnlyList<
                                                System.Collections.Generic.IDictionary<string,
                                                    System.Collections.Generic.IList<
                                                        System.Collections.Generic.IReadOnlyDictionary<string, int>>>>>>>>>>;
namespace MongoDB.Client.Tests.Serialization.Collections
{
    class CombinedComparer : IEqualityComparer<CombinedType>
    {
        public bool Equals(CombinedType x, CombinedType y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] CombinedType obj)
        {
            return obj.GetHashCode();
        }
    }
    [BsonSerializable]
    public partial class CollectionsCombineTestModel : IEquatable<CollectionsCombineTestModel>
    {
        public CombinedType Collections;
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

        public override bool Equals(object obj)
        {
            return Equals(obj as CollectionsCombineTestModel);
        }

        public bool Equals(CollectionsCombineTestModel other)
        {
            foreach (var item0 in Collections)
            {
                foreach (var otherItem0 in other.Collections)
                {
                    if (item0 is null || otherItem0 is null || item0.Count != otherItem0.Count)
                    {
                        return false;
                    }
                    foreach (var item1 in item0)
                    {
                        foreach (var otherItem1 in otherItem0)
                        {
                            if (item1.Key.Equals(otherItem1.Key) == false || item1.Value.Count != otherItem1.Value.Count)
                            {
                                return false;
                            }
                            foreach (var item2 in item1.Value)
                            {
                                foreach (var otherItem2 in other.Collections)
                                {
                                    if (item2 is null || otherItem2 is null || item2.Count != otherItem2.Count)
                                    {
                                        return false;
                                    }
                                    foreach (var item3 in item0)
                                    {
                                        foreach (var otherItem3 in otherItem0)
                                        {
                                            if (item3.Key.Equals(otherItem3.Key) == false || item3.Value.Count != otherItem3.Value.Count)
                                            {
                                                return false;
                                            }
                                            foreach (var item4 in item3.Value)
                                            {
                                                foreach (var otherItem4 in otherItem3.Value)
                                                {
                                                    if (item4 is null || otherItem4 is null || item4.Count != otherItem4.Count)
                                                    {
                                                        return false;
                                                    }
                                                    foreach (var item5 in item4)
                                                    {
                                                        foreach (var otherItem5 in otherItem4)
                                                        {
                                                            if (item5 is null || otherItem5 is null || item5.Count != otherItem5.Count)
                                                            {
                                                                return false;
                                                            }
                                                            foreach (var item6 in item5)
                                                            {
                                                                foreach (var otherItem6 in otherItem5)
                                                                {
                                                                    if (item6.Key.Equals(otherItem6.Key) == false || item6.Value.Count != otherItem6.Value.Count)
                                                                    {
                                                                        return false;
                                                                    }
                                                                    foreach (var item7 in item6.Value)
                                                                    {
                                                                        foreach (var otherItem7 in otherItem6.Value)
                                                                        {
                                                                            if (item7 is null || otherItem7 is null || item7.Count != otherItem7.Count)
                                                                            {
                                                                                return false;
                                                                            }
                                                                            foreach (var item8 in item7)
                                                                            {
                                                                                foreach (var otherItem8 in otherItem7)
                                                                                {
                                                                                    if (item8.Key.Equals(otherItem8.Key) == false || item8.Value.Count != otherItem8.Value.Count)
                                                                                    {
                                                                                        return false;
                                                                                    }
                                                                                    foreach (var item9 in item8.Value)
                                                                                    {
                                                                                        foreach (var otherItem9 in otherItem8.Value)
                                                                                        {
                                                                                            if (item9.TryGetValue("42", out var itemKey) == false ||
                                                                                                otherItem9.TryGetValue("42", out var otherItemKey) == false ||
                                                                                                itemKey != otherItemKey)
                                                                                            {
                                                                                                return false;
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
                                        }
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
            return HashCode.Combine(Collections);
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
            Assert.Equal(model, result);
        }

    }
}
