using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal interface IReplicaSetScheduler : IMongoScheduler
    {
        bool IsMaster { get; }
    }
}
