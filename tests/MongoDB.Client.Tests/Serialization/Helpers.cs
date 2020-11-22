using System.Collections.Generic;

namespace MongoDB.Client.Tests.Serialization
{
    public static class Helpers
    {
        public static bool SequentialEquals<T>(this IReadOnlyList<T> list1, IReadOnlyList<T> list2)
        {
            if (ReferenceEquals(list1, list2))
            {
                return true;
            }
            if (list1 is null && list2 is not null)
            {
                return false;
            }
            
            if (list1 is not null && list2 is null)
            {
                return false;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] == null && list2[i] == null)
                {
                    return true;
                }
                if (list1[i].Equals(list2[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}