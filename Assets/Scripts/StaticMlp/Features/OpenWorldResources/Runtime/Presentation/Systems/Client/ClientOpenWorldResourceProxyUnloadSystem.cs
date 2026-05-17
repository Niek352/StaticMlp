using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceProxyUnloadSystem : ISystem
    {
        private readonly List<EntityGID> _toDestroy = new();
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<OpenWorldChunkUnloadEvent>> _unloads;

        public void Init()
        {
            _unloads = CW.RegisterEventReceiver<NetworkEventFromServer<OpenWorldChunkUnloadEvent>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _unloads);
            _toDestroy.Clear();
        }

        public void Update()
        {
            var proxyIndex = CW.GetResource<ClientOpenWorldResourceProxyIndex>();
            foreach (var unload in _unloads)
                DestroyChunkProxies(unload.Value.Value.ChunkId, proxyIndex);
        }

        private void DestroyChunkProxies(
            WorldChunkId chunkId,
            ClientOpenWorldResourceProxyIndex proxyIndex)
        {
            _toDestroy.Clear();
            foreach (var entity in CW.Query<All<OpenWorldResourceProxyTag, OpenWorldResourceProxyRef>>().Entities())
            {
                ref readonly var proxyRef = ref entity.Read<OpenWorldResourceProxyRef>();
                if (proxyRef.ChunkX == chunkId.X && proxyRef.ChunkZ == chunkId.Z)
                    _toDestroy.Add(entity.GID);
            }

            for (var i = 0; i < _toDestroy.Count; i++)
            {
                if (!_toDestroy[i].TryUnpack<ClientCoreWT>(out var entity))
                    continue;

                var placementId = entity.Read<OpenWorldResourceProxyRef>().PlacementId;
                proxyIndex.Unregister(placementId);
                entity.Destroy();
            }

            _toDestroy.Clear();
        }
    }
}
