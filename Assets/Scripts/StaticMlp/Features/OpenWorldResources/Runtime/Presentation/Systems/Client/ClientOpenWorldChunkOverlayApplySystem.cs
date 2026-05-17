using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldChunkOverlayApplySystem : ISystem
    {
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<OpenWorldChunkOverlayAbsolute>> _absolutes;
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<OpenWorldChunkOverlayDelta>> _deltas;

        public void Init()
        {
            _absolutes = CW.RegisterEventReceiver<NetworkEventFromServer<OpenWorldChunkOverlayAbsolute>>();
            _deltas = CW.RegisterEventReceiver<NetworkEventFromServer<OpenWorldChunkOverlayDelta>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _absolutes);
            CW.DeleteEventReceiver(ref _deltas);
        }

        public void Update()
        {
            var overlayStore = CW.GetResource<OpenWorldChunkOverlayStore>();
            var proxyIndex = CW.GetResource<ClientOpenWorldResourceProxyIndex>();

            foreach (var evt in _absolutes)
                ApplyAbsolute(overlayStore, proxyIndex, evt.Value.Value);

            foreach (var evt in _deltas)
                ApplyDelta(overlayStore, proxyIndex, evt.Value.Value);
        }

        private static void ApplyAbsolute(
            OpenWorldChunkOverlayStore overlayStore,
            ClientOpenWorldResourceProxyIndex proxyIndex,
            OpenWorldChunkOverlayAbsolute msg)
        {
            overlayStore.ReplaceAbsolute(msg.ChunkId, msg.Revision, msg.ResourceStates);
            for (var i = 0; i < msg.ResourceStates.Length; i++)
                ApplyResourceStateToProxy(proxyIndex, msg.ResourceStates[i]);

            CW.SendToServer(new OpenWorldChunkOverlayAck
            {
                ChunkId = msg.ChunkId,
                Revision = msg.Revision
            });
        }

        private static void ApplyDelta(
            OpenWorldChunkOverlayStore overlayStore,
            ClientOpenWorldResourceProxyIndex proxyIndex,
            OpenWorldChunkOverlayDelta msg)
        {
            var localRevision = overlayStore.GetRevision(msg.ChunkId);
            if (localRevision != msg.BasisRevision)
            {
                CW.SendToServer(new OpenWorldChunkOverlayRequest
                {
                    ChunkId = msg.ChunkId,
                    KnownRevision = localRevision
                });
                return;
            }

            overlayStore.ApplyDelta(msg.ChunkId, msg.BasisRevision, msg.Revision, msg.ResourceDeltas);
            for (var i = 0; i < msg.ResourceDeltas.Length; i++)
                ApplyResourceDeltaToProxy(proxyIndex, msg.ResourceDeltas[i]);

            CW.SendToServer(new OpenWorldChunkOverlayAck
            {
                ChunkId = msg.ChunkId,
                Revision = msg.Revision
            });
        }

        private static void ApplyResourceStateToProxy(
            ClientOpenWorldResourceProxyIndex proxyIndex,
            OpenWorldResourceOverlayState state)
        {
            if (!proxyIndex.TryGet(state.PlacementId, out var gid))
                return;
            if (!gid.TryUnpack<ClientCoreWT>(out var entity))
            {
                proxyIndex.Unregister(state.PlacementId);
                return;
            }

            ref var proxyState = ref entity.Mut<OpenWorldResourceProxyState>();
            proxyState.KindIdValue = state.KindIdValue;
            proxyState.RemainingAmount = state.RemainingAmount;
            proxyState.Flags = state.Flags;

            ref var viewState = ref entity.Mut<OpenWorldResourceNodeViewState>();
            viewState.KindIdValue = state.KindIdValue;
            viewState.RemainingAmount = state.RemainingAmount;
        }

        private static void ApplyResourceDeltaToProxy(
            ClientOpenWorldResourceProxyIndex proxyIndex,
            OpenWorldResourceOverlayDelta delta)
        {
            if (!proxyIndex.TryGet(delta.PlacementId, out var gid))
                return;
            if (!gid.TryUnpack<ClientCoreWT>(out var entity))
            {
                proxyIndex.Unregister(delta.PlacementId);
                return;
            }

            ref var proxyState = ref entity.Mut<OpenWorldResourceProxyState>();
            proxyState.RemainingAmount = delta.RemainingAmount;
            proxyState.Flags = delta.Flags;

            ref var viewState = ref entity.Mut<OpenWorldResourceNodeViewState>();
            viewState.RemainingAmount = delta.RemainingAmount;
        }
    }
}
