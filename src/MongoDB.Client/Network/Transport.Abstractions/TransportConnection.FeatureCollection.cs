using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;

#nullable enable

namespace MongoDB.Client.Network.Transport.Abstractions
{
    internal partial class TransportConnection : IConnectionIdFeature,
                                                 IConnectionTransportFeature,
                                                 IConnectionItemsFeature,
                                                 IMemoryPoolFeature,
                                                 IConnectionLifetimeFeature
    {
        // NOTE: When feature interfaces are added to or removed from this TransportConnection class implementation,
        // then the list of `features` in the generated code project MUST also be updated.
        // See also: tools/CodeGenerator/TransportConnectionFeatureCollection.cs

        MemoryPool<byte> IMemoryPoolFeature.MemoryPool => MemoryPool;

        IDuplexPipe IConnectionTransportFeature.Transport
        {
            get => Transport;
            set => Transport = value;
        }

        IDictionary<object, object?> IConnectionItemsFeature.Items
        {
            get => Items;
            set => Items = value;
        }

        CancellationToken IConnectionLifetimeFeature.ConnectionClosed
        {
            get => ConnectionClosed;
            set => ConnectionClosed = value;
        }

        void IConnectionLifetimeFeature.Abort() => Abort(new ConnectionAbortedException("The connection was aborted by the application via IConnectionLifetimeFeature.Abort()."));
    }
}
