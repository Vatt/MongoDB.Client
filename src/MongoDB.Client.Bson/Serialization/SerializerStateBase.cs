using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Serialization
{
    abstract class SerializerStateBase
    {
        public int? DocLen;
        public int Consumed;
        public SequencePosition Position;
    }
}
