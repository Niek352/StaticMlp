using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ClientOpenWorldChunkUnloadSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<OpenWorldChunkUnloadEvent>> _unloads;

        public void Init()
        {
            _unloads = CW.RegisterEventReceiver<NetworkEventFromServer<OpenWorldChunkUnloadEvent>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _unloads);
        }

        public void Update()
        {
            foreach (var unload in _unloads)
                UnloadCluster(unload.Value.Value.ClusterId, unload.Value.ReceiveOrder);
        }

        private void UnloadCluster(ushort clusterId, int receiveOrder)
        {
            if (HasNewerSnapshotForCluster(clusterId, receiveOrder))
                return;

            CW.DestroyAllEntitiesInCluster(clusterId);
        }

        private static bool HasNewerSnapshotForCluster(ushort clusterId, int receiveOrder)
        {
            ref var inbox = ref CW.GetResource<NetInbox>();
            for (var i = 0; i < inbox.Snapshots.Count; i++)
                if (inbox.Snapshots[i].ClusterId == clusterId && inbox.Snapshots[i].ReceiveOrder > receiveOrder)
                    return true;

            return false;
        }
    }
}
