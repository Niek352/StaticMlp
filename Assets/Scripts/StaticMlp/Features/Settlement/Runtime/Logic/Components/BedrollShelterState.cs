using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "1dbd74af-ce56-4e7f-9220-df2aea692e5e"
    )]
    public partial struct BedrollShelterState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public byte SlotCount;

        [ReplicatedField]
        public bool Enabled;
    }
}
