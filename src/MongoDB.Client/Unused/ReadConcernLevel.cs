namespace MongoDB.Client.Unused
{
    /// <summary>
    /// The level of the read concern.
    /// </summary>
    public enum ReadConcernLevel
    {
        Default,

        /// <summary>
        /// Reads available data.
        /// </summary>
        Available,

        /// <summary>
        /// Reads data committed locally.
        /// </summary>
        Local,

        /// <summary>
        /// Reads data committed to a majority of nodes.
        /// </summary>
        Majority,

        /// <summary>
        /// Avoids returning data from a "stale" primary 
        /// (one that has already been superseded by a new primary but doesn't know it yet). 
        /// It is important to note that readConcern level linearizable does not by itself 
        /// produce linearizable reads; they must be issued in conjunction with w:majority 
        /// writes to the same document(s) in order to be linearizable.
        /// </summary>
        Linearizable,

        /// <summary>
        /// Snapshot read concern level.
        /// </summary>
        Snapshot
    }
}
