﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Serialization
{
    public abstract class SerializerStateBase
    {
        public int? DocLen;
        public long Consumed;
        public SerializerStateBase()
        {
            DocLen = null;
            Consumed = 0;
        }
    }
}
