using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "d37b9b22-ea96-49e0-bc20-4ab248c04c98"
    )]
    public partial struct ProductionStationOperationState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort StationId;

        [ReplicatedField]
        public ushort ActiveRecipeId;

        [ReplicatedField]
        public bool Enabled;

        [ReplicatedField]
        public byte WorkerSlotCount;

        [ReplicatedField]
        public float WorkDone;

        public ProductionStationId Station => new(StationId);
        public ProductionRecipeId ActiveRecipe => new(ActiveRecipeId);
    }
}
