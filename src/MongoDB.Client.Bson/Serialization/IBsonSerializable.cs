using MongoDB.Client.Bson.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializable
    {
        bool TryParse(ref MongoDBBsonReader reader,  out object message);
        
        void Write(object message);
    }
}
