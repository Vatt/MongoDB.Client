namespace MongoDB.Client.Messages
{
    public class InsertResult : IParserResult
    {
        public int N { get; set; }
        public double Ok { get; set; }

        public string? ErrorMessage { get; set; }
        public int Code { get; set; }
        public string? CodeName { get; set; }
    }
}