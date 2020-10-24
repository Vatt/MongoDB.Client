namespace MongoDB.Client.Messages
{
    public class CursorOwner
    {
        public Cursor Cursor { get; set; }

        public double Ok { get; set; }
    }

    public class Cursor
    {
        public long Id { get; set; }

        public string Namespace { get; set; }
    }
}
