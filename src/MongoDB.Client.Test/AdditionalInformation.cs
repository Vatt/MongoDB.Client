using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    [BsonSerializable]
    class ErrorData
    {
        [BsonElementField]
        public string ErrorMessage;
    }
    [BsonSerializable]
    class Information
    {
        [BsonElementField]
        public String Id;

        [BsonElementField]
        public string MessageType;

        [BsonElementField]
        public string Path;

        [BsonElementField]
        public string Filename;

        [BsonElementField]
        public string ServicePhase;

        [BsonElementField]
        public long   ServicePhaseDuration;
        
        [BsonElementField]
        public string LocationInformationId;

        [BsonElementField]
        public string DataInformationId;

        [BsonElementField]
        public string StorageKey;

        [BsonElementField]
        public long FileLength;

        [BsonElementField]
        public object BinaryDeleted;

        [BsonElementField]
        public string InputDataAccountingIdentifier;

        [BsonElementField]
        public string OutputData;

        [BsonElementField]
        public string OutputDataAccountingIdentifier;

        [BsonElementField]
        public string Direction;

        [BsonElementField]
        public string Source;

        [BsonElementField]
        public long Value1;

        [BsonElementField]
        public long Value2;

        [BsonElementField]
        public long Value3;

        [BsonElementField]
        public long Value4;

        [BsonElementField]
        public string ChannelId;
        
        [BsonElementField]
        public object TranscriberResultsStorageKey;

        [BsonElementField]
        public object PreprocessedResultsStorageKey;

        [BsonElementField]
        public object BookmarksStorageKey;

        [BsonElementField]
        public bool InTranscribing;

        [BsonElementField]
        public object InTranscribingDate;

        [BsonElementField]
        public object Result;

        [BsonElementField]
        public object ErrorData;


    }

    [BsonSerializable]
    class AdditionalInformation
    {
        [BsonElementField]
        public Guid TypeId;   
        [BsonElementField(ElementName = "_id")]
        public Guid Id;
        [BsonElementField]
        public List<Information> Informations;
    }
}
