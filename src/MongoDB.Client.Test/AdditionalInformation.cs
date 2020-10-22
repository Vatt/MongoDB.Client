//using MongoDB.Client.Bson.Document;
//using MongoDB.Client.Bson.Serialization.Attributes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MongoDB.Client.Test
//{
//    [BsonSerializable]
//    struct ErrorData
//    {

//        public string ErrorMessage;
//    }
//    [BsonSerializable]
//    class Information
//    {
//        public string Id;
//        public string MessageType { get; set; }


//        public string Path { get; set; }


//        public string FileName { get; set; }
//        public string Data { get; set; }

//        public DateTimeOffset DateTime;

//        public string DataTypeId;

//        public string InputData;

//        public Guid InputDataAccountingIdentifier;


//        public Guid OutputDataAccountingIdentifier;


//        public string OutputData;
//        public string Direction;
//        public string Source;


//        public string Filename { get; set; }


//        public string ServicePhase { get; set; }


//        public long   ServicePhaseDuration { get; set; }

//        public long Duration;
//        public string LocationInformationId { get; set; }


//        public string DataInformationId { get; set; }


//        public string StorageKey { get; set; }


//        public long FileLength { get; set; }


//        public bool BinaryDeleted { get; set; }

//        public long Value1 { get; set; }


//        public long Value2 { get; set; }


//        public long Value3 { get; set; }


//        public long Value4 { get; set; }


//        public string ChannelId { get; set; }


//        public string TranscriberResultsStorageKey { get; set; }


//        public Guid PreprocessedResultsStorageKey { get; set; }


//        public Guid BookmarksStorageKey { get; set; }


//        public bool InTranscribing { get; set; }


//        public DateTimeOffset InTranscribingDate { get; set; }

//        public Guid InTranscribingOpGuid { get; set; }
//        public string Result { get; set; }


//        public ErrorData ErrorData { get; set; }


//    }

//    [BsonSerializable]
//    class AdditionalInformation
//    {

//        public Guid TypeId { get; set; }

//        [BsonElementField(ElementName = "_id")]
//        public Guid Id { get; set; }

//        public List<Information> Informations { get; set; }
//        //public List<BsonDocument>? Informations;
//    }
//}
