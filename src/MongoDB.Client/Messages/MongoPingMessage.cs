using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class MongoPingMessage
    {
        [BsonElement("hosts")]
        public List<string> Hosts { get; }

        [BsonElement("setName")]
        public string SetName { get; }

        [BsonElement("me")]
        public string Me { get; }

        [BsonElement("primary")]
        public string Primary { get; }

        [BsonElement("ismaster")]
        public bool IsMaster { get; }

        [BsonElement("secondary")]
        public bool IsSecondary { get; }
        public MongoPingMessage(List<string> Hosts, string SetName, string Me, string Primary, bool IsMaster, bool IsSecondary)
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
