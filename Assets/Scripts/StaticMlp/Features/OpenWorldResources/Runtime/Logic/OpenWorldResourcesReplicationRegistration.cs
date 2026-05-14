using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldResourcesReplicationRegistration
    {
        public static void Register()
        {
            ReplicationRegistry.RegisterComponent<OpenWorldResourceNodeState>(
                OpenWorldResourceNodeStateReplication.TYPE_ID,
                OpenWorldResourceNodeStateReplication.AUTHORITY,
                OpenWorldResourceNodeStateReplication.AUDIENCE,
                OpenWorldResourceNodeStateReplication.DELIVERY,
                OpenWorldResourceNodeStateReplication.CreateDelta,
                OpenWorldResourceNodeStateReplication.Read);

            ReplicationRegistry.RegisterComponent<OpenWorldResourceNodeTransform>(
                OpenWorldResourceNodeTransformReplication.TYPE_ID,
                OpenWorldResourceNodeTransformReplication.AUTHORITY,
                OpenWorldResourceNodeTransformReplication.AUDIENCE,
                OpenWorldResourceNodeTransformReplication.DELIVERY,
                OpenWorldResourceNodeTransformReplication.CreateDelta,
                OpenWorldResourceNodeTransformReplication.Read);

            ReplicationRegistry.RegisterNetworkEntity(9, 1, OpenWorldResourceNetworkArchetypeIds.ResourceNode);
        }
    }
}
