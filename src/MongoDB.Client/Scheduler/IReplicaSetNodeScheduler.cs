using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal interface IReplicaSetNodeScheduler : IMongoScheduler
    {
        //ValueTask<bool> IsMaster();
    }
}
