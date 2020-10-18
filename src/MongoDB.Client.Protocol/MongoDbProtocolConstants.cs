using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Protocol
{
    public enum MongoDbProtocolConstants : int
    {
        OP_REPLY = 1,
        OP_UPDATE = 2001,
        OP_INSERT = 2002,
        RESERVED = 2003,
        OP_QUERY = 2004,
        OP_GET_MORE = 2005,
        OP_DELETE = 2006,
        OP_KILL_CURSORS = 2007,
        OP_MSG = 2013,
    }
}
