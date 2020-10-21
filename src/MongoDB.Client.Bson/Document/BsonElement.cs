using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MongoDB.Client.Bson.Document
{


    public readonly struct BsonElement
    {
        public readonly BsonDocument Parent { get; }
        public readonly BsonElementType Type { get; }
        public readonly string Name { get; }

        public object? Value { get; }
        //public readonly object? Value => _value;

        private BsonElement(BsonDocument parent, BsonElementType type, string name, object? value)
        {
            Parent = parent;
            Type = type;
            Name = name;
            Value = value;
        }
        //public unsafe T As<T>() where T: struct
        //{
        //    var ptr = Unsafe.AsPointer(ref _value);
        //    var val = Unsafe.AsRef<T>(ptr);
        //    return val;
        //}
        //public static BsonElement Create(BsonDocument parent, string name, byte value)
        //{
        //    return new BsonElement(parent, BsonElementType.Byte, name, value);
        //}
        //public static BsonElement Create(BsonDocument parent, string name, short value)
        //{
        //    return new BsonElement(parent, BsonElementType.Int16, name, value);
        //}
        public static BsonElement Create(BsonDocument parent, string name, int value)
        {
            return new BsonElement(parent, BsonElementType.Int32, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, long value)
        {
            return new BsonElement(parent, BsonElementType.Int64, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, string value)
        {
            return new BsonElement(parent, BsonElementType.String, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, double value)
        {
            return new BsonElement(parent, BsonElementType.Double, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, BsonDocument value)
        {
            return new BsonElement(parent, BsonElementType.Document, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, BsonObjectId value)
        {
            return new BsonElement(parent, BsonElementType.ObjectId, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, BsonBinaryData value)
        {
            return new BsonElement(parent, BsonElementType.BinaryData, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, DateTimeOffset value)
        {
            return new BsonElement(parent, BsonElementType.UTCDateTime, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name, bool value)
        {
            return new BsonElement(parent, BsonElementType.Boolean, name, value);
        }
        public static BsonElement Create(BsonDocument parent, string name)
        {
            return new BsonElement(parent, BsonElementType.Null, name, null);
        }
        public static BsonElement CreateArray(BsonDocument parent, string name, BsonDocument root)
        {
            return new BsonElement(parent, BsonElementType.Array, name, root);
        }

        public override string ToString()
        {
            return Name + ": " + (Value is not null ? Value.ToString() : "null");
        }
    }
}
