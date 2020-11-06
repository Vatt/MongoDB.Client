using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public enum AtmosphereType
    {
        [BsonElementField(ElementName = "No atmosphere")]
        NoAtmosphere,
        
        [BsonElementField(ElementName = "Hot thick silicate vapour")]
        HotThickSilicateVapour,
        
        [BsonElementField(ElementName = "Hot thick carbon dioxide")]
        HotThickCarbonDioxide,
        
        [BsonElementField(ElementName = "Hot thick Water")]
        HotThickWater,
        
        [BsonElementField(ElementName = "Sulphur dioxide")]
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