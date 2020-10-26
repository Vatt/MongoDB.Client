
//using MongoDB.Client.Bson.Reader;
//using MongoDB.Client.Bson.Writer;
//using MongoDB.Client.Bson.Serialization;
//using MongoDB.Client.Bson.Document;
//using System;
//using System.Collections.Generic;
//using MongoDB.Client;
//using MongoDB.Client.Test;
//using System.Runtime.CompilerServices;


//namespace MongoDB.Client.Bson.Serialization.Generated
//{

  
//    public class MongoTopologyVersionGeneratedSerializer<T0, T1, T2> : IGenericBsonSerializer<GenericDTO<T0, T1, T2>>
//    {

//        private unsafe readonly ref struct GenericT0Wrapper
//        {
//            public readonly void* Wrapped;
//            public GenericT0Wrapper(T0 genericval)
//            {
//                Wrapped = Unsafe.AsPointer(ref genericval);
//            }
//        };
//        private static ReadOnlySpan<byte> MongoTopologyVersionprocessId => new byte[9] { 112, 114, 111, 99, 101, 115, 115, 73, 100 };
//        private static ReadOnlySpan<byte> MongoTopologyVersioncounter => new byte[7] { 99, 111, 117, 110, 116, 101, 114 };
//        public MongoTopologyVersionGeneratedSerializer() { }
//        void IBsonSerializer.Write(object message) { throw new NotImplementedException(); }

//        bool  IGenericBsonSerializer<GenericDTO<T0, T1, T2>>.TryParse(ref MongoDBBsonReader reader, out GenericDTO<T0, T1, T2> message) 
//        {
//            message = default;

//            var result1 = new GenericDTO<T0, T1, T2>();
//            unsafe
//            {
//                GenericT0Wrapper wrapper = new GenericT0Wrapper(result1.Value00);
//                reader.TryGetInt32(out *(int*)wrapper.Wrapped); 
//            }
             
//            var result = new MongoTopologyVersion();
//            if (!reader.TryGetInt32(out var docLength)) { return false; }   
//            var unreaded = reader.Remaining + sizeof(int);
//            while (unreaded - reader.Remaining < docLength - 1)
//            {
//                if (!reader.TryGetByte(out var bsonType)) { return false; }
//                if (!reader.TryGetCStringAsSpan(out var bsonName)) { return false; }
                
//                if (bsonName.SequenceEqual(MongoTopologyVersionprocessId))
//                {
//                    if (bsonType == 10)
//                    {
//                        result.ProcesssId = default;
//                        continue;
//                    }

//                    if (!reader.TryGetObjectId(out var value)) { return false; }
//                    result.ProcesssId = value;

//                    continue;
//                }

//                if (bsonName.SequenceEqual(MongoTopologyVersioncounter))
//                {
//                    if (bsonType == 10)
//                    {
//                        result.Counter = default;     
//                        continue;
//                    }

//                    if (!reader.TryGetInt64(out var value)) { return false; }
//                    result.Counter = value;

//                    continue;
//                }


//                throw new ArgumentException($"MongoTopologyVersion.TryParse  with bson type number {bsonType}");
//            }
//            if (!reader.TryGetByte(out var endMarker)) { return false; }
//            if (endMarker != '\x00')
//            {
//                throw new ArgumentException("MongoTopologyVersionGeneratedSerializator.TryParse End document marker missmatch");
//            }

//            message = result;
//            return true;
//        }


//    }


//}