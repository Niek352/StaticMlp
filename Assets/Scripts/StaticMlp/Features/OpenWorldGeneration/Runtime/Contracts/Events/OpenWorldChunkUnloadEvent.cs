using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct OpenWorldChunkUnloadEvent : IEvent, IEventConfig<OpenWorldChunkUnloadEvent>
    {
        public const ushort NETWORK_EVENT_ID = 59031;

        public WorldChunkId ChunkId;
        public ushort ClusterId;

        public OpenWorldChunkUnloadEvent(WorldChunkId chunkId, ushort clusterId)
        {
            ChunkId = chunkId;
            ClusterId = clusterId;
        }

        public EventTypeConfig<OpenWorldChunkUnloadEvent> Config() =>
            new(guid: new Guid("998f596e-435b-4b10-b557-03b292444df2"));
    }
}
