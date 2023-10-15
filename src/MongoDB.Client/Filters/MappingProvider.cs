using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Filters
{
    internal static class MappingProvider<T> where T : IBsonSerializer<T>
    {
        public static readonly IReadOnlyDictionary<string, string> Mapping;
        static MappingProvider()
        {
            if (TryGetGenerated(out Mapping) is false)
            {
                Mapping = BuildMapping();
            }
        }
        private static bool TryGetGenerated([NotNullWhen(true)] out IReadOnlyDictionary<string, string> mapping)
        {
            var fieldInfo = typeof(T).GetField("__MAPPING__", BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo == null)
            {
                mapping = default;
                return false;
            }

            mapping = (IReadOnlyDictionary<string, string>)fieldInfo.GetValue(null)!;
            return mapping != null;
        }
        private static IReadOnlyDictionary<string, string> BuildMapping()
        {
            var mapping = new Dictionary<string, string>();

            foreach(var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = field.GetCustomAttribute(typeof(BsonElementAttribute)) as BsonElementAttribute;

                if (attribute != null)
                {
                    mapping.Add(field.Name, attribute.ElementName);
                }
                else
                {
                    mapping.Add(field.Name, field.Name);
                }
                
            }
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = property.GetCustomAttribute(typeof(BsonElementAttribute)) as BsonElementAttribute;

                if (attribute != null)
                {
                    mapping.Add(property.Name, attribute.ElementName);
                }
                else
                {
                    mapping.Add(property.Name, property.Name);
                }

            }

            return mapping;
        }
    }
}
