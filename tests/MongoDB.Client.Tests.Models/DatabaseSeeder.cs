using System.Linq;

namespace MongoDB.Client.Tests.Models
{
    public class DatabaseSeeder
    {
        public IEnumerable<T> GenerateSeed<T>(int count = 1024, bool lazy = false)
        {
            var seeder = CreateSeeder<T>();
            var enumerable = new GeoIpSeeder().GenerateSeed(count).Select(d => (T)(object)d);
            return lazy ? enumerable : enumerable.ToArray();
            
        }

        private static ISeeder<T> CreateSeeder<T>()
        {
            if (typeof(T) == typeof(RootDocument))
            {
                return (ISeeder<T>)new RootDocumentSeeder();
            }
            if (typeof(T) == typeof(GeoIp))
            {
                return (ISeeder<T>)new GeoIpSeeder();
            }
            if (typeof(T) == typeof(MediumModel))
            {
                return (ISeeder<T>)new MediumModelSeeder();
            }
            throw new NotSupportedException();
        }
    }
}
