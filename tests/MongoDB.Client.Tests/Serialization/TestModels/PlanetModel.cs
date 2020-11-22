using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonEnumSerializable(EnumRepresentation.String)]
    public enum AtmosphereType
    {
        [BsonElement("No atmosphere")]
        NoAtmosphere,
        
        [BsonElement("Hot thick silicate vapour")]
        HotThickSilicateVapour,
        
        [BsonElement("Hot thick carbon dioxide")]
        HotThickCarbonDioxide,
        
        [BsonElement("Hot thick Water")]
        HotThickWater,
        
        [BsonElement("Sulphur dioxide")]
        SulphurDioxide,

        [BsonIgnore]
        Ignoring,
    }
    
    [BsonSerializable]
    public class PlanetModel
    {
        public string Name { get; set; }
        public AtmosphereType Type;
        public override bool Equals(object obj)
        {
            return obj is not null && obj is PlanetModel other && Name.Equals(other.Name) && Type == other.Type; 
        }
    }
}