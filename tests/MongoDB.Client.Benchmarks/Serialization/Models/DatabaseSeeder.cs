using System;
using System.Collections.Generic;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    public class DatabaseSeeder
    {
        public List<RootDocument> GenerateSeed(int count = 500)
        {
            Console.WriteLine("Seeding a database for experiment....");

            const int innerDocumentOneCount = 50;
            const int innerDocumentTwoCount = 25;
            const int innerDocumentThreeCount = 5;

            var generatedDocuments = new List<RootDocument>();
            for (var i = 0; i < count; i++)
            {
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

                generatedDocuments.Add(complexDocument);
            }
            return generatedDocuments;
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
                SomeEnumField = (SomeEnum)(i % 3)
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
                FloatField = i,
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
