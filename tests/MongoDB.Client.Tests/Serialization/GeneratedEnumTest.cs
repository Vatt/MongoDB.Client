﻿using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
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
            SerializersMap.TryGetSerializer<PlanetModel>(out var serializer);
            var result = await RoundTripAsync(somePlanet, serializer);

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
            SerializersMap.TryGetSerializer<NumericEnumsModel>(out var serializer);
            var result = await RoundTripAsync(somePlanet, serializer);

            Assert.Equal(somePlanet, result);
        }
    }
}