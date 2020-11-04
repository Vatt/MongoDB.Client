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
}