using System;
using System.Threading;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;

#nullable disable

namespace MongoDB.Client.Unused
{
    public class Find<T>
    {
        private bool? _allowDiskUse;
        private bool? _allowPartialResults;
        private int? _batchSize;
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private string _comment;
        private CursorType _cursorType;
        private BsonDocument _filter;
        private int? _firstBatchSize;
        private BsonDocument _hint;
        private int? _limit;
        private BsonDocument _max;
        private TimeSpan? _maxAwaitTime;
        private TimeSpan? _maxTime;
        //private readonly MessageEncoderSettings _messageEncoderSettings;
        private BsonDocument _min;
        private bool? _noCursorTimeout;
        private BsonDocument _projection;
        private ReadConcern _readConcern = ReadConcern.Default;
        //private readonly IBsonSerializer<TDocument> _resultSerializer;
        private bool _retryRequested;
        private bool? _returnKey;
        private bool? _showRecordId;
        private bool? _singleBatch;
        private int? _skip;
        private BsonDocument _sort;

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether the server is allowed to write to disk while executing the Find operation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server is allowed to write to disk while executing the Find operation; otherwise, <c>false</c>.
        /// </value>
        public bool? AllowDiskUse
        {
            get { return _allowDiskUse; }
            set { _allowDiskUse = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is allowed to return partial results if any shards are unavailable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server is allowed to return partial results if any shards are unavailable; otherwise, <c>false</c>.
        /// </value>
        public bool? AllowPartialResults
        {
            get { return _allowPartialResults; }
            set { _allowPartialResults = value; }
        }

        /// <summary>
        /// Gets or sets the size of a batch.
        /// </summary>
        /// <value>
        /// The size of a batch.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or sets the type of the cursor.
        /// </summary>
        /// <value>
        /// The type of the cursor.
        /// </value>
        public CursorType CursorType
        {
            get { return _cursorType; }
            set { _cursorType = value; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the size of the first batch.
        /// </summary>
        /// <value>
        /// The size of the first batch.
        /// </value>
        public int? FirstBatchSize
        {
            get { return _firstBatchSize; }
            set { _firstBatchSize = value; }
        }

        /// <summary>
        /// Gets or sets the hint.
        /// </summary>
        /// <value>
        /// The hint.
        /// </value>
        public BsonDocument Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        public int? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets or sets the max key value.
        /// </summary>
        /// <value>
        /// The max key value.
        /// </value>
        public BsonDocument Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the maximum await time for TailableAwait cursors.
        /// </summary>
        /// <value>
        /// The maximum await time for TailableAwait cursors.
        /// </value>
        public TimeSpan? MaxAwaitTime
        {
            get { return _maxAwaitTime; }
            set { _maxAwaitTime = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }

        ///// <summary>
        ///// Gets the message encoder settings.
        ///// </summary>
        ///// <value>
        ///// The message encoder settings.
        ///// </value>
        //public MessageEncoderSettings MessageEncoderSettings
        //{
        //    get { return _messageEncoderSettings; }
        //}

        /// <summary>
        /// Gets or sets the min key value.
        /// </summary>
        /// <value>
        /// The max min value.
        /// </value>
        public BsonDocument Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server will not timeout the cursor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server will not timeout the cursor; otherwise, <c>false</c>.
        /// </value>
        public bool? NoCursorTimeout
        {
            get { return _noCursorTimeout; }
            set { _noCursorTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>
        /// The projection.
        /// </value>
        public BsonDocument Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        /// <summary>
        /// Gets or sets the read concern.
        /// </summary>
        /// <value>
        /// The read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = value; }
        }

        ///// <summary>
        ///// Gets the result serializer.
        ///// </summary>
        ///// <value>
        ///// The result serializer.
        ///// </value>
        //public IBsonSerializer<TDocument> ResultSerializer
        //{
        //    get { return _resultSerializer; }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether to retry.
        /// </summary>
        /// <value>Whether to retry.</value>
        public bool RetryRequested
        {
            get => _retryRequested;
            set => _retryRequested = value;
        }

        /// <summary>
        /// Gets or sets whether to only return the key values.
        /// </summary>
        /// <value>
        /// Whether to only return the key values.
        /// </value>
        public bool? ReturnKey
        {
            get { return _returnKey; }
            set { _returnKey = value; }
        }

        /// <summary>
        /// Gets or sets whether the record Id should be added to the result document.
        /// </summary>
        /// <value>
        /// Whether the record Id should be added to the result documentr.
        /// </value>
        public bool? ShowRecordId
        {
            get { return _showRecordId; }
            set { _showRecordId = value; }
        }

        /// <summary>
        /// Gets or sets whether to return only a single batch.
        /// </summary>
        /// <value>
        /// Whether to return only a single batchThe single batch.
        /// </value>
        public bool? SingleBatch
        {
            get { return _singleBatch; }
            set { _singleBatch = value; }
        }

        /// <summary>
        /// Gets or sets the number of documents skip.
        /// </summary>
        /// <value>
        /// The number of documents skip.
        /// </value>
        public int? Skip
        {
            get { return _skip; }
            set { _skip = value; }
        }

        /// <summary>
        /// Gets or sets the sort specification.
        /// </summary>
        /// <value>
        /// The sort specification.
        /// </value>
        public BsonDocument Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }



        internal BsonDocument CreateCommand(ConnectionInfo connectionInfo, Session session)
        {
            //Feature.ReadConcern.ThrowIfNotSupported(connectionDescription.ServerVersion, _readConcern);
            //Feature.Collation.ThrowIfNotSupported(connectionDescription.ServerVersion, _collation);

            var firstBatchSize = _firstBatchSize ?? (_batchSize > 0 ? _batchSize : null);
            //    var isShardRouter = connectionDescription.IsMasterResult.ServerType == ServerType.ShardRouter;
            var isShardRouter = false;

            var readConcern = ReadConcernHelper.GetReadConcern(session, connectionInfo, _readConcern);
            return new BsonDocument
            {
                { "find", _collectionNamespace.CollectionName },
                { "filter", _filter, _filter != null },
                { "sort", _sort, _sort != null },
                { "projection", _projection, _projection != null },
                { "hint", _hint, _hint != null },
                { "skip", _skip, _skip.HasValue },
                { "limit", () => Math.Abs(_limit.Value), _limit.HasValue && _limit != 0 },
                { "batchSize", firstBatchSize, firstBatchSize.HasValue },
                { "singleBatch", () => _limit < 0 || _singleBatch.Value, _limit < 0 || _singleBatch.HasValue },
                { "comment", _comment, _comment != null },
                { "maxTimeMS", () => ToMaxTimeMS(_maxTime.Value), _maxTime.HasValue },
                { "max", _max, _max != null },
                { "min", _min, _min != null },
                { "returnKey", () => _returnKey.Value, _returnKey.HasValue },
                { "showRecordId", () => _showRecordId.Value, _showRecordId.HasValue },
                { "tailable", true, _cursorType == CursorType.Tailable || _cursorType == CursorType.TailableAwait },
                { "noCursorTimeout", () => _noCursorTimeout.Value, _noCursorTimeout.HasValue },
                { "awaitData", true, _cursorType == CursorType.TailableAwait },
                { "allowDiskUse", () => _allowDiskUse.Value, _allowDiskUse.HasValue },
                { "allowPartialResults", () => _allowPartialResults.Value, _allowPartialResults.HasValue && isShardRouter },
                { "collation", () => _collation.ToBsonDocument(), _collation != null },
                { "readConcern", readConcern, readConcern != null }
            };
        }

        public static int ToMaxTimeMS(TimeSpan value)
        {
            if (value == Timeout.InfiniteTimeSpan)
            {
                return 0;
            }
            else if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else
            {
                return (int)Math.Ceiling(value.TotalMilliseconds);
            }
        }
    }
}
#nullable restore
