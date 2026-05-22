using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "81cc0b9a-2497-4926-8561-ac701776a1a9"
    )]
    public partial struct ExtractionOperationState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort OutputResourceId;

        [ReplicatedField]
        public int OutputBufferAmount;

        [ReplicatedField]
        public ushort OutputBufferCapacity;

        [ReplicatedField]
        public bool Enabled;

        [ReplicatedField]
        public byte WorkerSlotCount;

        public ResourceId OutputResource => new(OutputResourceId);
    }
}
