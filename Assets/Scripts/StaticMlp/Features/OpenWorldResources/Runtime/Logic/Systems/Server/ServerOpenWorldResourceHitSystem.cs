using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceHitSystem : ISystem
    {
        private EventReceiver<ServerWT, CombatTargetHitEvent> _hits;

        public void Init()
        {
            _hits = SW.RegisterEventReceiver<CombatTargetHitEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _hits);
        }

        public void Update()
        {
            var placementIndex = SW.GetResource<OpenWorldPlacementIndexStore>();
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();

            foreach (var hit in _hits)
            {
                var evt = hit.Value;
                if (evt.Target.Kind != CombatTargetKind.StaticPlacement)
                    continue;

                ApplyHit(in evt, placementIndex, overlayStore);
            }
        }

        private static void ApplyHit(
            in CombatTargetHitEvent evt,
            OpenWorldPlacementIndexStore placementIndex,
            OpenWorldChunkOverlayStore overlayStore)
        {
            var target = evt.Target;
            if (!placementIndex.TryGetPlacement(target.PlacementId, out var placement))
                throw new InvalidOperationException($"Cannot apply resource hit: placement {target.PlacementId} is not registered in the authority placement index.");
            if (!overlayStore.TryGetChunkId(target.PlacementId, out var chunkId))
                throw new InvalidOperationException($"Cannot apply resource hit: placement {target.PlacementId} is not registered in the authority overlay store.");
            if (chunkId != placement.ChunkId)
                throw new InvalidOperationException($"Cannot apply resource hit: placement {target.PlacementId} is indexed in chunk {placement.ChunkId} but overlay store resolves chunk {chunkId}.");

            var currentState = overlayStore.GetEffectiveResourceState(placement);
            if (OpenWorldResourceNodeRules.IsDepleted(currentState))
                return;

            var nextState = OpenWorldResourceNodeRules.ApplyHit(
                placement,
                currentState,
                out var harvestedResource,
                out var wasDepleted);

            if (!overlayStore.TryApplyResourceState(chunkId, nextState))
                throw new InvalidOperationException($"Resource hit for placement {target.PlacementId} did not change overlay state.");

            SW.SendEvent(new OpenWorldResourceHarvestedEvent
            {
                SourcePlayer = evt.Source,
                PlacementId = target.PlacementId,
                Resource = harvestedResource,
                HitPointXQ = target.HitPointXQ,
                HitPointYQ = target.HitPointYQ,
                HitPointZQ = target.HitPointZQ,
                WasDepleted = wasDepleted
            });

            if (wasDepleted)
            {
                SW.SendEvent(new OpenWorldResourceDepletionHazardEvent
                {
                    SourcePlayer = evt.Source,
                    PlacementId = target.PlacementId,
                    KindId = placement.KindId,
                    Origin = placement.Position
                });
            }
        }
    }
}
