namespace MongoDB.Client.Unused
{
    /// <summary>
    /// Represents the type of a cluster.
    /// </summary>
    public enum ClusterType
    {
        /// <summary>
        /// The type of the cluster is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The cluster is a standalone cluster.
        /// </summary>
        Standalone,

        /// <summary>
        /// The cluster is a replica set.
        /// </summary>
        ReplicaSet,

        /// <summary>
        /// The cluster is a sharded cluster.
        /// </summary>
        Sharded
    }
}
