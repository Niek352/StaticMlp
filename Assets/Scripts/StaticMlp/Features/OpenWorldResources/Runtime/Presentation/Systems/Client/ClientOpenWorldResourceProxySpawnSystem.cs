using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceProxySpawnSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, OpenWorldChunkGenerationCompleted> _completed;

        public void Init()
        {
            _completed = CW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _completed);
        }

        public void Update()
        {
            var placementIndex = CW.GetResource<OpenWorldPlacementIndexStore>();
            var overlayStore = CW.GetResource<OpenWorldChunkOverlayStore>();
            var proxyIndex = CW.GetResource<ClientOpenWorldResourceProxyIndex>();

            foreach (var evt in _completed)
            {
                var chunk = evt.Value;
                if (chunk.ResourcePlacements.Length == 0)
                    continue;

                placementIndex.RegisterChunkPlacements(chunk.ChunkId, chunk.ResourcePlacements);
                for (var i = 0; i < chunk.ResourcePlacements.Length; i++)
                {
                    var placement = chunk.ResourcePlacements[i];
                    overlayStore.RegisterPlacement(chunk.ChunkId, placement.PlacementId);
                    if (proxyIndex.TryGet(placement.PlacementId, out _))
                        continue;

                    var state = overlayStore.GetEffectiveResourceState(placement);
                    if (IsInactive(state.Flags))
                        continue;

                    CreateProxy(placement, state, proxyIndex);
                }

                CW.SendToServer(new OpenWorldChunkOverlayRequest
                {
                    ChunkId = chunk.ChunkId,
                    KnownRevision = overlayStore.GetRevision(chunk.ChunkId)
                });
            }
        }

        private static void CreateProxy(
            ResourcePlacement placement,
            OpenWorldResourceOverlayState state,
            ClientOpenWorldResourceProxyIndex proxyIndex)
        {
            var entity = ClientOnlyEntities.New();
            entity.Set<OpenWorldResourceProxyTag>();
            entity.Set(new OpenWorldResourceProxyRef
            {
                PlacementId = placement.PlacementId,
                ChunkX = placement.ChunkId.X,
                ChunkZ = placement.ChunkId.Z
            });
            entity.Set(new OpenWorldResourceProxyState
            {
                KindIdValue = state.KindIdValue,
                RemainingAmount = state.RemainingAmount,
                Flags = state.Flags
            });
            entity.Set(new ViewTransform
            {
                RenderPosition = placement.Position,
                RenderRotation = Quaternion.Euler(0f, placement.YawDegrees, 0f)
            });
            entity.Set(new OpenWorldResourceNodeViewState
            {
                KindIdValue = state.KindIdValue,
                RemainingAmount = state.RemainingAmount,
                MaxAmount = OpenWorldResourceNodeRules.StartingAmount(placement.KindId),
                Flags = state.Flags,
                Scale = placement.Scale
            });
            proxyIndex.Register(placement.PlacementId, entity.GID);
        }

        private static bool IsInactive(OpenWorldResourceOverlayFlags flags)
        {
            const OpenWorldResourceOverlayFlags inactive =
                OpenWorldResourceOverlayFlags.Depleted
                | OpenWorldResourceOverlayFlags.Hidden
                | OpenWorldResourceOverlayFlags.Replaced;
            return (flags & inactive) != 0;
        }
    }
}
