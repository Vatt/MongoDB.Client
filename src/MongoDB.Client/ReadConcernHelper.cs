using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;

namespace MongoDB.Client
{
    internal static class ReadConcernHelper
    {
        public static BsonDocument GetReadConcern(Session session, ConnectionInfo connectionInfo, ReadConcern readConcern)
        {
            return session.IsInTransaction ? null : null;
        }

        //private static BsonDocument ToBsonDocument(Session session, ConnectionDescription connectionDescription, ReadConcern readConcern)
        //{
        //    var sessionsAreSupported = connectionDescription.IsMasterResult.LogicalSessionTimeout != null;
        //    var shouldSendAfterClusterTime = sessionsAreSupported && session.IsCausallyConsistent && session.OperationTime != null;
        //    var shouldSendReadConcern = !readConcern.IsServerDefault || shouldSendAfterClusterTime;

        //    if (shouldSendReadConcern)
        //    {
        //        var readConcernDocument = readConcern.ToBsonDocument();
        //        if (shouldSendAfterClusterTime)
        //        {
        //            readConcernDocument.Add("afterClusterTime", session.OperationTime);
        //        }
        //        return readConcernDocument;
        //    }

        //    return null;
        //}
    }
}
