using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Models
{
    public class RootDocumentSeeder : SeederBase<RootDocument>
    {
        protected override RootDocument Create(uint i)
        {
            const int innerDocumentOneCount = 50;
            const int innerDocumentTwoCount = 25;
            const int innerDocumentThreeCount = 5;

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

        private RootDocument CreateTestComplexDocument(uint i)
        {
            return new RootDocument
            {
                Id = BsonObjectId.NewObjectId(),
                DoubleField = i,
                IntField = (int) i,
                TextFieldOne = $"{i}_{i}_{i}",
                TextFieldTwo = $"{i}-{i}-{i}",
                TextFieldThree = $"{i}|{i}|{i}",
                SomeEnumField = (SomeEnum)(i % 3),
                Update = "old"
            };
        }

        private FirstLevelDocument CreateTestInnerDocumentOne(uint i)
        {
            return new FirstLevelDocument
            {
                IntField = (int)i,
                TextField = $"*{i}*"
            };
        }

        private SecondLevelDocument CreateTestInnerDocumentTwo(uint i)
        {
            return new SecondLevelDocument
            {
                IntField = (int)i,
                TextField = $"**{i}**"
            };
        }

        private ThirdLevelDocument CreateTestInnerDocumentThree(uint i)
        {
            return new ThirdLevelDocument
            {
                DoubleField = i,
                TextField = $"***{i}***"
            };
        }
    }
}
