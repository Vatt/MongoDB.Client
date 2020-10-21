namespace MongoDB.Client.Protocol.Readers
{
    public enum Opcode
    {
        Reply = 1,
        Message = 1000,
        Update = 2001,
        Insert = 2002,
        Query = 2004,
        GetMore = 2005,
        Delete = 2006,
        KillCursors = 2007,
        Compressed = 2012,
        OpMsg = 2013
    }
}
