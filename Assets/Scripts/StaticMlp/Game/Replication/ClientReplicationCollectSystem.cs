using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientReplicationCollectSystem : ISystem
    {
        public void Update()
        {
            ref var outbox = ref CW.GetResource<NetOutbox>();
            ReplicationRegistry.CollectClientOwnedDirty(outbox, new NetworkPeerId(0));
        }
    }
}
