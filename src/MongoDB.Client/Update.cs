using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client
{ 
    public interface IUpdateDocument{
    
    }
    [BsonSerializable]
    public readonly partial struct UpdateUnset<T> : IUpdateDocument
    {
        [BsonElement("$setOnInsert")]
        public T Unset { get; }

        public UpdateUnset(T unset)
        {
            Unset = unset;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateSetOrInsert<T> : IUpdateDocument
    {
        [BsonElement("$setOnInsert")]
        public T SetOnInsert { get; }

        public UpdateSetOrInsert(T setOnInsert)
        {
            SetOnInsert = setOnInsert;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateRename<T> : IUpdateDocument
    {
        [BsonElement("$rename")]
        public T Rename { get; }

        public UpdateRename(T rename)
        {
            Rename = rename;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateMul<T> : IUpdateDocument
    {
        [BsonElement("$mul")]
        public T Mul { get; }

        public UpdateMul(T mul)
        {
            Mul = mul;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateMax<T> : IUpdateDocument
    {
        [BsonElement("$max")]
        public T Max { get; }

        public UpdateMax(T max)
        {
            Max = max;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateMin<T> : IUpdateDocument
    {
        [BsonElement("$min")]
        public T Min { get; }

        public UpdateMin(T min)
        {
            Min = min;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateInc<T> : IUpdateDocument
    {
        [BsonElement("$inc")]
        public T Inc { get; }

        public UpdateInc(T inc)
        {
            Inc = inc;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateSet<T> : IUpdateDocument
    {
        [BsonElement("$set")]
        public T Set { get; }

        public UpdateSet(T set)
        {
            Set = set;
        }
    }
    
    [BsonSerializable]
    public readonly partial struct UpdateCurrentDate<T> : IUpdateDocument
    {
        [BsonElement("$currentDate")]
        public T CurrentDate { get; }

        public UpdateCurrentDate(T currentDate)
        {
            CurrentDate = currentDate;
        }
    }
    
    public partial class Update
    {
        private  delegate void NameWriter(ref BsonWriter writter);
        private delegate void DocWriter(ref BsonWriter writer);
        
        private static ReadOnlySpan<byte> UpdateInc => new byte[4]{36, 105, 110, 99};
        private static ReadOnlySpan<byte> UpdateMax => new byte[4]{36, 109, 97, 120};
        private static ReadOnlySpan<byte> UpdateMin => new byte[4]{36, 109, 105, 110};
        private static ReadOnlySpan<byte> UpdateMul => new byte[4]{36, 109, 117, 108};
        private static ReadOnlySpan<byte> UpdateRename => new byte[7]{36, 114, 101, 110, 97, 109, 101};
        private static ReadOnlySpan<byte> UpdateSetOrInsert => new byte[12]{36, 115, 101, 116, 79, 110, 73, 110, 115, 101, 114, 116};
        private static ReadOnlySpan<byte> UpdateSet => new byte[4]{36, 115, 101, 116};
        private static ReadOnlySpan<byte> UpdateUnset => new byte[12]{36, 115, 101, 116, 79, 110, 73, 110, 115, 101, 114, 116};

        private DocWriter _document;
        private NameWriter _nameWriter;
        public static Update Set<T>(T value)
        {
            var update = new Update()
            {
                _nameWriter = (ref BsonWriter writer) => writer.WriteName(UpdateSet),
                _document = (ref BsonWriter writer) =>
                {
                    unsafe
                    {
                        SerializerFnPtrProvider<T>.WriteFnPtr(ref writer, value);
                    }
                },
            };
            //BsonElement doc = BsonElement.Create(default, expr())
            return update;
        }
        
        
        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Update message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            message._nameWriter(ref writer);
            message._document(ref writer);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }

        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Update message)
        {
            throw new NotSupportedException($"{nameof(Update)}.{nameof(TryParseBson)}");
        }
    }
}
