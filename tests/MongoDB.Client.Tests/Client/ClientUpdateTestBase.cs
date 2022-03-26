using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial class FirstDoc
    {
        [BsonId]
        public BsonObjectId Id { get; set; }
        public string TextFieldOne { get; set; }

        public string TextFieldTwo { get; set; }

        public string TextFieldThree { get; set; }
        public int IntField { get; set; }
        public List<SecondDoc> Documents { get; set; }

        public static FirstDoc Create()
        {
            List<SecondDoc> documents = new() {SecondDoc.Create(21), SecondDoc.Create(22), SecondDoc.Create(23)};
            return new()
            {
                Id = BsonObjectId.NewObjectId(),
                TextFieldOne = "One",
                TextFieldTwo = "Two",
                TextFieldThree = "Three",
                IntField = 42,
                Documents = documents
            };
        }
    }
    
    [BsonSerializable]
    public partial class SecondDoc
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdDoc> Documents { get; set; }

        public static SecondDoc Create(int value)
        {
            List<ThirdDoc> documents = new() {ThirdDoc.Create(3.1), ThirdDoc.Create(3.2), ThirdDoc.Create(3.3)};
            return new() {TextField = "SecodDocTextField", IntField = value, Documents = documents};
        }
    }
    
    [BsonSerializable]
    public partial class ThirdDoc
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }

        public static ThirdDoc Create(double value)
        {
            return new() {TextField = "ThirdDocTextField", DoubleField = value};
        }
    }

    [BsonSerializable]
    public partial class UpdateFirstDoc
    {
        public string TextFieldOne { get; set; }
        public int IntField { get; set; }
        public List<UpateSecondDoc> Documents { get; set; }

        public static UpdateFirstDoc Create()
        {
            List<UpateSecondDoc> documents = new() {UpateSecondDoc.Create(), UpateSecondDoc.Create()};
            return new()
            {
                TextFieldOne = "UpdatedOne",
                IntField = 24,
                Documents = documents
            };
        }
    }

    [BsonSerializable]
    public partial class UpateSecondDoc
    {
        public string TextField { get; set; }
        public List<UpdateThirdDoc> Documents { get; set; }

        public static UpateSecondDoc Create()
        {
            List<UpdateThirdDoc> documents = new() {UpdateThirdDoc.Create(), UpdateThirdDoc.Create()};
            return new() {TextField = "UpdatedSecodDocTextField", Documents = documents};
        }
    }

    [BsonSerializable]
    public partial class UpdateThirdDoc
    {
        public string TextField { get; set; }
        public static UpdateThirdDoc Create()
        {
            return new() {TextField = "UpdatedThirdDocTextField"};
        }
    }
    public class ClientUpdateTestBase : ClientTestBase
    {
        
        public async Task UpdateOne_Set(MongoClient client)
        {
            var items = new[] {FirstDoc.Create(), FirstDoc.Create()};
            var update = Update.Set(UpdateFirstDoc.Create());
            var filter = BsonDocument.Empty;
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<FirstDoc>("UpdateSetCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await UpdateOneAsync(items, filter, update, collection);
            await collection.DropAsync();
        }
    }
}
