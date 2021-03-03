using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Messages
{
    //[BsonSerializable]
    public partial class MongoPingMessage
    {
        [BsonElement("hosts")]
        public List<EndPoint> Hosts { get; }

        [BsonElement("setName")]
        public string SetName { get; }

        [BsonElement("me")]
        public EndPoint Me { get; }

        [BsonElement("primary")]
        public EndPoint? Primary { get; }

        [BsonElement("ismaster")]
        public bool IsMaster { get; }

        [BsonElement("secondary")]
        public bool IsSecondary { get; }
        public MongoPingMessage(List<EndPoint> Hosts, string SetName, EndPoint Me, EndPoint Primary, bool IsMaster, bool IsSecondary)
        {
            this.Hosts = Hosts;
            this.SetName = SetName;
            this.Me = Me;
            this.Primary = Primary;
            this.IsMaster = IsMaster;
            this.IsSecondary = IsSecondary;
        }
        private static readonly byte ColonChar = (byte)':';
        private static ReadOnlySpan<byte> MongoPingMessagehosts => new byte[5] { 104, 111, 115, 116, 115 };
        private static ReadOnlySpan<byte> MongoPingMessagesetName => new byte[7] { 115, 101, 116, 78, 97, 109, 101 };
        private static ReadOnlySpan<byte> MongoPingMessageme => new byte[2] { 109, 101 };
        private static ReadOnlySpan<byte> MongoPingMessageprimary => new byte[7] { 112, 114, 105, 109, 97, 114, 121 };
        private static ReadOnlySpan<byte> MongoPingMessageismaster => new byte[8] { 105, 115, 109, 97, 115, 116, 101, 114 };
        private static ReadOnlySpan<byte> MongoPingMessagesecondary => new byte[9] { 115, 101, 99, 111, 110, 100, 97, 114, 121 };
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, [NotNullWhen(true)]out MongoDB.Client.Messages.MongoPingMessage message)
        {
            message = default;
            System.Collections.Generic.List<EndPoint> ListHosts = default;
            string StringSetName = default;
            EndPoint StringMe = default;
            EndPoint StringPrimary = default;
            bool BooleanIsMaster = default;
            bool BooleanIsSecondary = default;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }

            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                if (!reader.TryGetByte(out var bsonType))
                {
                    return false;
                }

                if (!reader.TryGetCStringAsSpan(out var bsonName))
                {
                    return false;
                }

                if (bsonType == 10)
                {
                    continue;
                }
                var temp = Encoding.UTF8.GetString(bsonName);
                if (bsonName.SequenceEqual(MongoPingMessagehosts))
                {
                    if (!TryParseHostsListString(ref reader, out ListHosts))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(MongoPingMessagesetName))
                {
                    if (!reader.TryGetString(out StringSetName))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(MongoPingMessageme))
                {
                    if (!TryReadEndpoint(ref reader, out StringMe))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(MongoPingMessageprimary))
                {
                    if (!TryReadEndpoint(ref reader, out StringPrimary))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(MongoPingMessageismaster))
                {
                    if (!reader.TryGetBoolean(out BooleanIsMaster))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(MongoPingMessagesecondary))
                {
                    if (!reader.TryGetBoolean(out BooleanIsSecondary))
                    {
                        return false;
                    }

                    continue;
                }

                if (!reader.TrySkip(bsonType))
                {
                    return false;
                }
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoPingMessage), endMarker);
            }

            message = new MongoDB.Client.Messages.MongoPingMessage(Hosts: ListHosts, SetName: StringSetName, Me: StringMe, Primary: StringPrimary, IsMaster: BooleanIsMaster, IsSecondary: BooleanIsSecondary);
            return true;
        }
        public static bool TryReadEndpoint(ref MongoDB.Client.Bson.Reader.BsonReader reader, out EndPoint endpoint)
        {
            endpoint = default;
            if (!reader.TryGetStringAsSpan(out var temp))
            {
                return false;
            }
            else
            {
                ReadOnlySpan<byte> host;
                ReadOnlySpan<byte> port;
                var index = temp.IndexOf(ColonChar);
                host = temp.Slice(0, index);
                port = temp.Slice(index + 1);
                var hostStr = Encoding.UTF8.GetString(host);
                var portStr = Encoding.UTF8.GetString(port);
                endpoint = new DnsEndPoint(hostStr, int.Parse(portStr));
                return true;
            }
        }
        private static bool TryParseHostsListString(ref MongoDB.Client.Bson.Reader.BsonReader reader, out System.Collections.Generic.List<EndPoint> array)
        {
            array = new System.Collections.Generic.List<EndPoint>();
            if (!reader.TryGetInt32(out int arrayDocLength))
            {
                return false;
            }

            var arrayUnreaded = reader.Remaining + sizeof(int);
            while (arrayUnreaded - reader.Remaining < arrayDocLength - 1)
            {
                if (!reader.TryGetByte(out var arrayBsonType))
                {
                    return false;
                }

                if (!reader.TrySkipCString())
                {
                    return false;
                }

                if (arrayBsonType == 10)
                {
                    array.Add(default);
                    continue;
                }

                if (!TryReadEndpoint(ref reader, out var endpoint))
                {
                    return false;
                }
                else
                {
                    array.Add(endpoint);
                    continue;
                }
            }

            if (!reader.TryGetByte(out var arrayEndMarker))
            {
                return false;
            }

            if (arrayEndMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Messages.MongoPingMessage), arrayEndMarker);
            }

            return true;
        }
        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Messages.MongoPingMessage message)
        {
            //var checkpoint = writer.Written;
            //var reserved = writer.Reserve(4);
            //if (message.Hosts == null)
            //{
            //    writer.WriteBsonNull(MongoPingMessagehosts);
            //}
            //else
            //{
            //    writer.Write_Type_Name(4, MongoPingMessagehosts);
            //    WriteHostsListString(ref writer, message.Hosts);
            //}

            //if (message.SetName == null)
            //{
            //    writer.WriteBsonNull(MongoPingMessagesetName);
            //}
            //else
            //{
            //    writer.Write_Type_Name_Value(MongoPingMessagesetName, message.SetName);
            //}

            //if (message.Me == null)
            //{
            //    writer.WriteBsonNull(MongoPingMessageme);
            //}
            //else
            //{
            //    writer.Write_Type_Name_Value(MongoPingMessageme, message.Me);
            //}

            //if (message.Primary == null)
            //{
            //    writer.WriteBsonNull(MongoPingMessageprimary);
            //}
            //else
            //{
            //    writer.Write_Type_Name_Value(MongoPingMessageprimary, message.Primary);
            //}

            //writer.Write_Type_Name_Value(MongoPingMessageismaster, message.IsMaster);
            //writer.Write_Type_Name_Value(MongoPingMessagesecondary, message.IsSecondary);
            //writer.WriteByte(0);
            //var docLength = writer.Written - checkpoint;
            //Span<byte> sizeSpan = stackalloc byte[4];
            //System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            //reserved.Write(sizeSpan);
            //writer.Commit();
            throw new NotSupportedException(nameof(MongoPingMessage));
        }
        private static void WriteHostsListString(ref MongoDB.Client.Bson.Writer.BsonWriter writer, System.Collections.Generic.List<string> array)
        {
            //var checkpoint = writer.Written;
            //var reserved = writer.Reserve(4);
            //for (var index = 0; index < array.Count; index++)
            //{
            //    if (array[index] == null)
            //    {
            //        writer.WriteBsonNull(index);
            //    }
            //    else
            //    {
            //        writer.Write_Type_Name_Value(index, array[index]);
            //    }
            //}

            //writer.WriteByte(0);
            //var docLength = writer.Written - checkpoint;
            //Span<byte> sizeSpan = stackalloc byte[4];
            //System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            //reserved.Write(sizeSpan);
            //writer.Commit();
            throw new NotSupportedException(nameof(MongoPingMessage));
        }

    }

}
