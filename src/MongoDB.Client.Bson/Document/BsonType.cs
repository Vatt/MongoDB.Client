namespace MongoDB.Client.Bson.Document
{
    public enum BsonType : byte
    {
        Double = 1,
        String = 2,
        Document = 3,
        Array = 4,
        BinaryData = 5,
        ObjectId = 7,
        Boolean = 8,
        UtcDateTime = 9,
        Null = 10,
        Int32 = 16,
        Timestamp = 17,
        Int64 = 18,
        Decimal = 19,
    }
}
