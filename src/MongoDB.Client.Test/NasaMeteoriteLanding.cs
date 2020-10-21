using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;


namespace MongoDB.Client.Test
{
    [BsonSerializable]
    class NasaMeteoriteLanding
    {
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        [BsonElementField(ElementName = "name")]
        public string Name { get; set; }

        [BsonElementField(ElementName = "nametype")]
        public string Nametype { get; set; }

        [BsonElementField(ElementName = "recclass")]
        public string Recclass { get; set; }

        [BsonElementField(ElementName = "mass (g)")]
        public string Mass_g { get; set; }

        [BsonElementField(ElementName = "fall")]
        public string Fall { get; set; }

        [BsonElementField(ElementName = "year")]
        public string Year { get; set; }

        [BsonElementField(ElementName = "reclat")]
        public string Reclat { get; set; }

        [BsonElementField(ElementName = "reclong")]
        public string Reclong { get; set; }

        public string GeoLocation { get; set; }
    }
}
