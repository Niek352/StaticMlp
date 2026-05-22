using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    /// <summary>
    /// Server-only component placed on a finished stockpile building entity.
    /// Records the storage capacity this individual building contributes to the settlement aggregate.
    /// </summary>
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "23efe9a1-a281-40d2-b288-c8dcdd3220e5"
    )]
    public partial struct StockpileOperationState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort ContributedCapacity;

        [ReplicatedField]
        public bool Enabled;
    }
}
