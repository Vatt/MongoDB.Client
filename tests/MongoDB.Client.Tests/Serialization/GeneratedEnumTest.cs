using MongoDB.Client.Tests.Serialization.TestModels;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedEnumTest : BaseSerialization
    {
        [Fact]
        public async Task EnumSerializationDeserialization()
        {
            var somePlanet = new PlanetModel
            {
                Name = "HUYADES SECTOR 33-4-12",
                Type = AtmosphereType.HotThickSilicateVapour,
            };
            var result = await RoundTripAsync(somePlanet);

            Assert.Equal(somePlanet, result);
        }
        [Fact]
        public async Task NumericEnumSerializationDeserialization()
        {
            var somePlanet = new NumericEnumsModel
            {
                Name = "HUYADES SECTOR 33-4-12",
                Int32EnumValue = Int32Enum.EnumInt32Value3,
                Int64EnumValue = Int64Enum.EnumInt64Value1,
            };
            var result = await RoundTripAsync(somePlanet);

            Assert.Equal(somePlanet, result);
        }
    }
}