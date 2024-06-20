using System.Linq.Expressions;

namespace MongoDB.Client.Filters
{
    public static class LinqExtensions
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source, int size, params Func<TSource, bool>[] predicates)
        {
            throw new InvalidOperationException($"Call function on client ({nameof(All)})");
        }
        public static bool All<TSource>(this IEnumerable<TSource> source, int size, params Func<TSource, bool>[] predicates)
        {
            throw new InvalidOperationException($"Call function on client ({nameof(All)})");
        }
    }
}
