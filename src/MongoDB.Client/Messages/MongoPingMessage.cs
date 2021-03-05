﻿using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MongoDB.Client.Messages
{
    public static class DnsEndPointSerializer
    {
        private static readonly byte ColonChar = (byte)':';
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out EndPoint message)
        {
            message = default;
            if (!reader.TryGetStringAsSpan(out var temp))
            {
                return false;
            }
            else
            {
                ReadOnlySpan<byte> host;
                ReadOnlySpan<byte> port;
                var index = temp.LastIndexOf(ColonChar);
                host = temp.Slice(0, index);
                port = temp.Slice(index + 1);
                var hostStr = Encoding.UTF8.GetString(host);
                var portStr = Encoding.UTF8.GetString(port);
                message = new DnsEndPoint(hostStr, int.Parse(portStr));
                return true;
            }
        }
        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in EndPoint message)
        {
            throw new NotSupportedException(nameof(DnsEndPointSerializer));
        }
    }
    [BsonSerializable]
    public partial class MongoPingMessage
    {
        [BsonElement("hosts")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public List<EndPoint> Hosts { get; }

        [BsonElement("setName")]
        public string SetName { get; }

        [BsonElement("me")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
        public EndPoint Me { get; }

        [BsonElement("primary")]
        [BsonSerializer(typeof(DnsEndPointSerializer))]
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
    }

}
