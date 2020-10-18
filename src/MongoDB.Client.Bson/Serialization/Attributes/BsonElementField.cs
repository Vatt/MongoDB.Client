using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BsonElementField : Attribute
    {
        public string ElementName { get; set; }
        public BsonElementField()
        {
            
        }
    }
}
