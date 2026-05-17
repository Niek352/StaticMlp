using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldChunkOverlayStore : IResource
    {
        private readonly Dictionary<WorldChunkId, OpenWorldChunkOverlay> _chunks = new();
        private readonly Dictionary<long, WorldChunkId> _chunkByPlacementId = new();
        private readonly OpenWorldChunkOverlayDirtyQueue _dirtyQueue;

        public OpenWorldChunkOverlayStore()
        {
        }

        public OpenWorldChunkOverlayStore(OpenWorldChunkOverlayDirtyQueue dirtyQueue)
        {
            _dirtyQueue = dirtyQueue;
        }

        public OpenWorldChunkOverlay GetOrCreate(WorldChunkId chunkId)
        {
            if (_chunks.TryGetValue(chunkId, out var overlay))
                return overlay;

            overlay = new OpenWorldChunkOverlay(chunkId);
            _chunks.Add(chunkId, overlay);
            return overlay;
        }

        public bool TryGet(WorldChunkId chunkId, out OpenWorldChunkOverlay overlay)
        {
            return _chunks.TryGetValue(chunkId, out overlay);
        }

        public bool TryGetResource(long placementId, out OpenWorldResourceOverlayState state)
        {
            if (_chunkByPlacementId.TryGetValue(placementId, out var chunkId)
                && _chunks.TryGetValue(chunkId, out var overlay)
                && overlay.TryGetResource(placementId, out state))
                return true;

            state = default;
            return false;
        }

        public bool TryGetChunkId(long placementId, out WorldChunkId chunkId)
        {
            return _chunkByPlacementId.TryGetValue(placementId, out chunkId);
        }

        public void RegisterPlacement(WorldChunkId chunkId, long placementId)
        {
            if (placementId == 0)
                throw new ArgumentOutOfRangeException(nameof(placementId), placementId, "Placement id must be non-zero.");

            if (_chunkByPlacementId.TryGetValue(placementId, out var existingChunkId) && existingChunkId != chunkId)
                throw new InvalidOperationException($"Placement {placementId} was already registered in chunk {existingChunkId}, not {chunkId}.");

            _chunkByPlacementId[placementId] = chunkId;
            GetOrCreate(chunkId);
        }

        public bool IsDepleted(long placementId)
        {
            return TryGetResource(placementId, out var state)
                   && (state.Flags & OpenWorldResourceOverlayFlags.Depleted) != 0;
        }

        public bool TryApplyResourceState(WorldChunkId chunkId, OpenWorldResourceOverlayState state)
        {
            RegisterPlacement(chunkId, state.PlacementId);
            var changed = GetOrCreate(chunkId).ApplyResourceState(state);
            if (changed)
                _dirtyQueue?.MarkDirty(chunkId);

            return changed;
        }

        public OpenWorldResourceOverlayState GetEffectiveResourceState(ResourcePlacement placement)
        {
            if (TryGetResource(placement.PlacementId, out var state))
            {
                if (state.KindIdValue == 0)
                    state.KindIdValue = placement.KindId.Value;

                return state;
            }

            return new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = checked((ushort)OpenWorldResourceNodeRules.StartingAmount(placement.KindId)),
                Flags = OpenWorldResourceOverlayFlags.None,
                RespawnTick = 0
            };
        }

        public uint GetRevision(WorldChunkId chunkId)
        {
            return _chunks.TryGetValue(chunkId, out var overlay) ? overlay.Revision : 0;
        }

        public OpenWorldChunkOverlay ReplaceAbsolute(
            WorldChunkId chunkId,
            uint revision,
            IReadOnlyList<OpenWorldResourceOverlayState> states)
        {
            var overlay = GetOrCreate(chunkId);
            overlay.ReplaceAbsolute(revision, states);

            for (var i = 0; i < states.Count; i++)
                RegisterPlacement(chunkId, states[i].PlacementId);

            return overlay;
        }

        public OpenWorldChunkOverlay ApplyDelta(
            WorldChunkId chunkId,
            uint basisRevision,
            uint revision,
            IReadOnlyList<OpenWorldResourceOverlayDelta> deltas)
        {
            var overlay = GetOrCreate(chunkId);
            overlay.ApplyResourceDeltas(basisRevision, revision, deltas);

            for (var i = 0; i < deltas.Count; i++)
                RegisterPlacement(chunkId, deltas[i].PlacementId);

            return overlay;
        }
    }
}
