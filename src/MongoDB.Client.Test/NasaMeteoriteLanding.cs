using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;


namespace MongoDB.Client.Test
{
    [BsonSerializable]
    class NasaMeteoriteLanding
    {
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id;

        [BsonElementField(ElementName = "name")]
        public string Name;

        [BsonElementField(ElementName = "nametype")]
        public string Nametype;

        [BsonElementField(ElementName = "recclass")]
        public string Recclass;
        
        [BsonElementField(ElementName = "mass (g)")]
        public string Mass_g;
        
        [BsonElementField(ElementName = "fall")]
        public string Fall;
        
        [BsonElementField(ElementName = "year")]
        public string Year;
        
        [BsonElementField(ElementName = "reclat")]
        public string Reclat;
        
        [BsonElementField(ElementName = "reclong")]
        public string Reclong;
        
        [BsonElementField]
        public string GeoLocation;
    }
}
