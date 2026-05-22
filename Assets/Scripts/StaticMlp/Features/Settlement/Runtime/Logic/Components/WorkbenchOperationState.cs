using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "934eb4df-f887-40dd-b8bf-8750466e74b8"
    )]
    public partial struct WorkbenchOperationState : IComponent, ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort ActiveRecipeId;

        [ReplicatedField]
        public bool Enabled;

        [ReplicatedField]
        public byte WorkerSlotCount;

        [ReplicatedField]
        public float WorkDone;

        public WorkbenchRecipeId ActiveRecipe => new(ActiveRecipeId);
    }
}
