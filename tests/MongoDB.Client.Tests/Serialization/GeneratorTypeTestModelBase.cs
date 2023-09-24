using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    public abstract class GeneratorTypeTestModelBase<T, TNullable>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;

        public T Property { get; }
        public TNullable NullableProperty { get; }
        public TNullable AlwaysNullProperty { get; }
        public List<T> ListProperty { get; }
        public List<T>? NullableListProperty { get; }
        public List<T>? AlwaysNullListProperty { get; }
        public List<TNullable> ListWithNullableTypeArgumentProperty { get; }
        public List<TNullable>? NullableListWithNullableTypeArgumentProperty { get; }
        public List<TNullable>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        public Dictionary<string, T> DictionaryProperty { get; }
        public Dictionary<string, T>? NullableDictionaryProperty { get; }
        public Dictionary<string, T>? AlwaysNullDictionaryProperty { get; }
        public Dictionary<string, TNullable> DictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, TNullable>? NullableDictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, TNullable>? AlwaysNullDictionaryWithNullableTypeArgument { get; }

        public GeneratorTypeTestModelBase(
            T property,
            TNullable nullableProperty,
            TNullable alwaysNullProperty,
            List<T> listProperty,
            List<T>? nullableListProperty,
            List<T>? alwaysNullListProperty,
            List<TNullable> listWithNullableTypeArgumentProperty,
            List<TNullable>? nullableListWithNullableTypeArgumentProperty,
            List<TNullable>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, T> dictionaryProperty,
            Dictionary<string, T>? nullableDictionaryProperty,
            Dictionary<string, T>? alwaysNullDictionaryProperty,
            Dictionary<string, TNullable> dictionaryWithNullableTypeArgument,
            Dictionary<string, TNullable>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, TNullable>? alwaysNullDictionaryWithNullableTypeArgument)
        {
            Property = property;
            NullableProperty = nullableProperty;
            AlwaysNullProperty = alwaysNullProperty;
            ListProperty = listProperty;
            NullableListProperty = nullableListProperty;
            AlwaysNullListProperty = alwaysNullListProperty;
            ListWithNullableTypeArgumentProperty = listWithNullableTypeArgumentProperty;
            NullableListWithNullableTypeArgumentProperty = nullableListWithNullableTypeArgumentProperty;
            AlwaysNullListWithNullableTypeArgumentProperty = alwaysNullListWithNullableTypeArgumentProperty;
            DictionaryProperty = dictionaryProperty;
            NullableDictionaryProperty = nullableDictionaryProperty;
            AlwaysNullDictionaryProperty = alwaysNullDictionaryProperty;
            DictionaryWithNullableTypeArgument = dictionaryWithNullableTypeArgument;
            NullableDictionaryWithNullableTypeArgument = nullableDictionaryWithNullableTypeArgument;
            AlwaysNullDictionaryWithNullableTypeArgument = alwaysNullDictionaryWithNullableTypeArgument;
        }

        public virtual bool Equals(BsonDocument doc)
        {
            foreach (var elem in doc)
            {
                switch (elem.Name)
                {
                    case "Property" or "NullableProperty":
                        {
                            if (elem.Type != BsonType)
                            {
                                return false;
                            }
                            break;
                        }
                    case "ListProperty" or "NulalbleListProperty" or "ListWithNullableTypeArgumentProperty" or "NullableListWithNullableTypeArgumentProperty":
                        {
                            if (elem.Type != BsonElementType.Array)
                            {
                                return false;
                            }
                            break;
                        }
                    case "DictionaryProperty" or "NullableDictionaryProperty" or "DictionaryWithNullableTypeArgument" or "NullableDictionaryWithNullableTypeArgument":
                        {
                            if (elem.Type != BsonElementType.Document)
                            {
                                return false;
                            }
                            break;
                        }


                }
            }
            return true;
        }
    }
}

