using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    public abstract class GeneratorTypeTestModelBase<T, TNulalble>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;
        
        public T Property { get; }
        public TNulalble NullableProperty { get; }
        public TNulalble AlwaysNullProperty { get; }
        public List<T> ListProperty { get; }
        public List<T>? NullableListProperty { get; }
        public List<T>? AlwaysNullListProperty { get; }
        public List<TNulalble> ListWithNullableTypeArgumentProperty { get; }
        public List<TNulalble>? NullableListWithNullableTypeArgumentProperty { get; }
        public List<TNulalble>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        public Dictionary<string, T> DictionaryProperty { get; }
        public Dictionary<string, T>? NullableDictionaryProperty { get; }
        public Dictionary<string, T>? AlwaysNullDictionaryProperty { get; }
        public Dictionary<string, TNulalble> DictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, TNulalble>? NullableDictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, TNulalble>? AlwaysNullDictionaryWithNullableTypeArgument { get; }
        public GeneratorTypeTestModelBase(
            T property, 
            TNulalble nullableProperty, 
            TNulalble alwaysNullProperty, 
            List<T> listProperty,
            List<T>? nullableListProperty,
            List<T>? alwaysNullListProperty,
            List<TNulalble> listWithNullableTypeArgumentProperty, 
            List<TNulalble>? nullableListWithNullableTypeArgumentProperty, 
            List<TNulalble>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, T> dictionaryProperty, 
            Dictionary<string, T>? nullableDictionaryProperty, 
            Dictionary<string, T>? alwaysNullDictionaryProperty, 
            Dictionary<string, TNulalble> dictionaryWithNullableTypeArgument, 
            Dictionary<string, TNulalble>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, TNulalble>? alwaysNullDictionaryWithNullableTypeArgument)
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
            foreach(var elem in doc)
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

