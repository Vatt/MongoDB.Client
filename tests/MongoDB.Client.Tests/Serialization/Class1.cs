
//using MongoDB.Client.Bson.Reader;
//using MongoDB.Client.Bson.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace MongoDB.Client.Bson.Serialization.Generated
//{
//    public static class GlobalSerializationHelperGenerated11
//    {

//        public static readonly IGenericBsonSerializer<MongoDB.Client.Messages.FindRequest> MongoDBClientMessagesFindRequestSerializerGeneratedStaticField = new MongoDBClientMessagesFindRequestSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.Messages.InsertHeader> MongoDBClientMessagesInsertHeaderSerializerGeneratedStaticField = new MongoDBClientMessagesInsertHeaderSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.Messages.InsertResult> MongoDBClientMessagesInsertResultSerializerGeneratedStaticField = new MongoDBClientMessagesInsertResultSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.Messages.InsertError> MongoDBClientMessagesInsertErrorSerializerGeneratedStaticField = new MongoDBClientMessagesInsertErrorSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.Messages.SessionId> MongoDBClientMessagesSessionIdSerializerGeneratedStaticField = new MongoDBClientMessagesSessionIdSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.MongoConnections.MongoTopologyVersion> MongoDBClientMongoConnectionsMongoTopologyVersionSerializerGeneratedStaticField = new MongoDBClientMongoConnectionsMongoTopologyVersionSerializerGenerated();
//        public static readonly IGenericBsonSerializer<MongoDB.Client.MongoConnections.MongoConnectionInfo> MongoDBClientMongoConnectionsMongoConnectionInfoSerializerGeneratedStaticField = new MongoDBClientMongoConnectionsMongoConnectionInfoSerializerGenerated();

//        public static KeyValuePair<Type, IBsonSerializer>[] GetGeneratedSerializers()
//        {
//            var pairs = new KeyValuePair<Type, IBsonSerializer>[7];

//            pairs[0] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.Messages.FindRequest), MongoDBClientMessagesFindRequestSerializerGeneratedStaticField);

//            pairs[1] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.Messages.InsertHeader), MongoDBClientMessagesInsertHeaderSerializerGeneratedStaticField);

//            pairs[2] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.Messages.InsertResult), MongoDBClientMessagesInsertResultSerializerGeneratedStaticField);

//            pairs[3] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.Messages.InsertError), MongoDBClientMessagesInsertErrorSerializerGeneratedStaticField);

//            pairs[4] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.Messages.SessionId), MongoDBClientMessagesSessionIdSerializerGeneratedStaticField);

//            pairs[5] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.MongoConnections.MongoTopologyVersion), MongoDBClientMongoConnectionsMongoTopologyVersionSerializerGeneratedStaticField);

//            pairs[6] = KeyValuePair.Create<Type, IBsonSerializer>(typeof(MongoDB.Client.MongoConnections.MongoConnectionInfo), MongoDBClientMongoConnectionsMongoConnectionInfoSerializerGeneratedStaticField);

//            return pairs;
//        }


//        [ModuleInitializerAttribute]
//        public static void MapInit()
//        {
//            MongoDB.Client.Bson.Serialization.SerializersMap.RegisterSerializers(GetGeneratedSerializers());
//        }
//    }
//}