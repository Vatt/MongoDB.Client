using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Unused
{
    public readonly struct ReadConcern
    {
        private readonly ReadConcernLevel _level;

        public ReadConcern(ReadConcernLevel level)
        {
            _level = level;
        }

        /// <summary>
        /// Gets an available read concern.
        /// </summary>
        public static ReadConcern Available => new ReadConcern(ReadConcernLevel.Available);

        /// <summary>
        /// Gets a default read concern.
        /// </summary>
        public static ReadConcern Default => new ReadConcern(ReadConcernLevel.Default);

        /// <summary>
        /// Gets a linearizable read concern.
        /// </summary>
        public static ReadConcern Linearizable => new ReadConcern(ReadConcernLevel.Linearizable);

        /// <summary>
        /// Gets a local read concern.
        /// </summary>
        public static ReadConcern Local => new ReadConcern(ReadConcernLevel.Local);

        /// <summary>
        /// Gets a majority read concern.
        /// </summary>
        public static ReadConcern Majority => new ReadConcern(ReadConcernLevel.Majority);

        /// <summary>
        /// Gets a snapshot read concern.
        /// </summary>
        public static ReadConcern Snapshot => new ReadConcern(ReadConcernLevel.Snapshot);

        /// <summary>
        /// Gets a value indicating whether this is the server's default read concern.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsServerDefault => _level == ReadConcernLevel.Default;

        public BsonDocument ToBson()
        {
            switch (_level)
            {
                case ReadConcernLevel.Available:
                    return _available;
                case ReadConcernLevel.Local:
                    return _local;
                case ReadConcernLevel.Majority:
                    return _majority;
                case ReadConcernLevel.Linearizable:
                    return _linearizable;
                case ReadConcernLevel.Snapshot:
                    return _snapshot;
                case ReadConcernLevel.Default:
                default:
                    return _default;
            }
        }

        private static readonly BsonDocument _default = new BsonDocument();
        private static readonly BsonDocument _available = new BsonDocument("level", "available");
        private static readonly BsonDocument _local = new BsonDocument("level", "local");
        private static readonly BsonDocument _majority = new BsonDocument("level", "majority");
        private static readonly BsonDocument _linearizable = new BsonDocument("level", "linearizable");
        private static readonly BsonDocument _snapshot = new BsonDocument("level", "snapshot");
    }
}
