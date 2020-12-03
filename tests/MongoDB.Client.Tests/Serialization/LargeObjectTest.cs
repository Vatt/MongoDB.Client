using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class LargeObjectTest : BaseSerialization
    {

        [Fact]
        public async Task LabgeObjectTest()
        {
            var model = GenerateObject();
            var result = await RoundTripAsync(model);

            Assert.Equal(model, result);
        }



        public RootDocument GenerateObject()
        {
            const int innerDocumentOneCount = 50;
            const int innerDocumentTwoCount = 25;
            const int innerDocumentThreeCount = 5;
            int i = 0;

            var complexDocument = CreateTestComplexDocument(i);
            complexDocument.InnerDocuments = new List<FirstLevelDocument>();

            for (var j = 0; j < innerDocumentOneCount; j++)
            {
                var firstLevelDocument = CreateTestInnerDocumentOne(i);
                firstLevelDocument.InnerDocuments = new List<SecondLevelDocument>();

                for (var k = 0; k < innerDocumentTwoCount; k++)
                {
                    var secondLevelDocument = CreateTestInnerDocumentTwo(i);
                    secondLevelDocument.InnerDocuments = new List<ThirdLevelDocument>();

                    for (var l = 0; l < innerDocumentThreeCount; l++)
                    {
                        var thirdLevelDocument = CreateTestInnerDocumentThree(i);
                        secondLevelDocument.InnerDocuments.Add(thirdLevelDocument);
                    }

                    firstLevelDocument.InnerDocuments.Add(secondLevelDocument);
                }

                complexDocument.InnerDocuments.Add(firstLevelDocument);
            }

            return complexDocument;
        }

        private RootDocument CreateTestComplexDocument(int i)
        {
            return new RootDocument
            {
                DoubleField = i,
                IntField = i,
                TextFieldOne = $"{i}_{i}_{i}",
                TextFieldTwo = $"{i}-{i}-{i}",
                TextFieldThree = $"{i}|{i}|{i}",
                SomeEnumField = (SomeEnum) (i % 3)
            };
        }

        private FirstLevelDocument CreateTestInnerDocumentOne(int i)
        {
            return new FirstLevelDocument
            {
                IntField = i,
                TextField = $"*{i}*"
            };
        }

        private SecondLevelDocument CreateTestInnerDocumentTwo(int i)
        {
            return new SecondLevelDocument
            {
                IntField = i,
                TextField = $"**{i}**"
            };
        }

        private ThirdLevelDocument CreateTestInnerDocumentThree(int i)
        {
            return new ThirdLevelDocument
            {
                DoubleField = i,
                TextField = $"***{i}***"
            };
        }
    }
}