using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class ManyListModel
    {
        public string Name;
        public List<List<List<List<List<List<long>>>>>> Longs;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return obj is ManyListModel other && Equals(Longs, other.Longs);
        }

        public static bool Equals(List<List<List<List<List<List<long>>>>>> list0,
            List<List<List<List<List<List<long>>>>>> list1)
        {
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
    }
}