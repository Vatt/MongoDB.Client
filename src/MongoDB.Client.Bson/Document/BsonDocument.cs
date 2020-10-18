using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Document
{
    public class BsonDocument
    {

        public readonly List<BsonElement> Elements;
       
        public BsonDocument()
        {
            Elements = new List<BsonElement>();            
        }
        
    }
}
