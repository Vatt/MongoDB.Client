using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;

namespace MongoDB.Client
{
    public class UpdateOptions
    {
        public Collation? Collation { get; }
        public List<BsonDocument>? ArrayFilters { get; }
        public bool IsUpsert { get; }
        public bool? BypassDocumentValidation { get; }

        public UpdateOptions(bool isUpsert = false, Collation? collation = null, List<BsonDocument>? arrayFilters = null, bool? bypassDocumentValidation = null)
        {
            Collation = collation;
            IsUpsert = isUpsert;
            ArrayFilters = arrayFilters;
            BypassDocumentValidation = bypassDocumentValidation;
        }
    }
    public class Update
    {
        private enum UpdateType
        {
            Inc,
            Max,
            Min,
            Mul,
            Rename,
            SetOnInsert,
            Set,
            Unset
        }
        private delegate void DocWriter(ref BsonWriter writer);
        
        private static ReadOnlySpan<byte> UpdateInc => new byte[4]{36, 105, 110, 99};
        private static ReadOnlySpan<byte> UpdateMax => new byte[4]{36, 109, 97, 120};
        private static ReadOnlySpan<byte> UpdateMin => new byte[4]{36, 109, 105, 110};
        private static ReadOnlySpan<byte> UpdateMul => new byte[4]{36, 109, 117, 108};
        private static ReadOnlySpan<byte> UpdateRename => new byte[7]{36, 114, 101, 110, 97, 109, 101};
        private static ReadOnlySpan<byte> UpdateSetOnInsert => new byte[12]{36, 115, 101, 116, 79, 110, 73, 110, 115, 101, 114, 116};
        private static ReadOnlySpan<byte> UpdateSet => new byte[4]{36, 115, 101, 116};
        private static ReadOnlySpan<byte> UpdateUnset => new byte[6]{36, 117, 110, 115, 101, 116};

        private readonly DocWriter _writer;
        private readonly UpdateType _type;

        private Update(UpdateType type, DocWriter writer)
        {
            _writer = writer;
            _type = type;
        }
        public static Update Set<T>(T value) => new Update(UpdateType.Set, MakeWriter(value));
        public static Update Unset<T>(T value) => new Update(UpdateType.Unset, MakeWriter(value));
        public static Update Unset(string name) => Unset(new BsonDocument(name, ""));
        public static Update Inc<T>(T value) => new Update(UpdateType.Inc, MakeWriter(value));
        public static Update Max<T>(T value) => new Update(UpdateType.Max, MakeWriter(value));
        public static Update Min<T>(T value) => new Update(UpdateType.Min, MakeWriter(value));
        public static Update Mul<T>(T value) => new Update(UpdateType.Mul, MakeWriter(value));
        public static Update SetOnInsert<T>(T value) => new Update(UpdateType.SetOnInsert, MakeWriter(value));
        public static Update Rename<T>(T value) => new Update(UpdateType.Rename, MakeWriter(value));
        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Update message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            WriteTypeName(ref writer, message._type);
            message._writer(ref writer);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
        private static void WriteTypeName(ref BsonWriter writer, UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Inc:
                    writer.Write_Type_Name(3, UpdateInc);
                    break;
                case UpdateType.Max:
                    writer.Write_Type_Name(3, UpdateMax);
                    break;
                case UpdateType.Min:
                    writer.Write_Type_Name(3, UpdateMin);
                    break;
                case UpdateType.Mul:
                    writer.Write_Type_Name(3, UpdateMul);
                    break;
                case UpdateType.Rename:
                    writer.Write_Type_Name(3, UpdateRename);
                    break;
                case UpdateType.SetOnInsert:
                    writer.Write_Type_Name(3, UpdateSetOnInsert);
                    break;
                case UpdateType.Set:
                    writer.Write_Type_Name(3, UpdateSet);
                    break;
                case UpdateType.Unset:
                    writer.Write_Type_Name(3, UpdateUnset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Update message)
        {
            throw new NotSupportedException($"{nameof(Update)}.{nameof(TryParseBson)}");
        }

        private static DocWriter MakeWriter<T>(T value)
        {
            return (ref BsonWriter writer) =>
            {
                unsafe
                {
                    SerializerFnPtrProvider<T>.WriteFnPtr(ref writer, value);
                }
            };
        }
    }
}
