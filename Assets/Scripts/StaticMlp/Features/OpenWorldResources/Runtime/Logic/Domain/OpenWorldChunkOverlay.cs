using System;
using System.Collections.Generic;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldChunkOverlay
    {
        private readonly Dictionary<long, OpenWorldResourceOverlayState> _resourceStates = new();
        private readonly List<OpenWorldResourceOverlayDelta> _dirtyResourceDeltas = new();
        private readonly List<OpenWorldResourceOverlayChange> _resourceChanges = new();

        public OpenWorldChunkOverlay(WorldChunkId chunkId)
        {
            ChunkId = chunkId;
        }

        public WorldChunkId ChunkId { get; }
        public uint Revision { get; private set; }

        public IReadOnlyDictionary<long, OpenWorldResourceOverlayState> ResourceStates => _resourceStates;
        public IReadOnlyList<OpenWorldResourceOverlayDelta> DirtyResourceDeltas => _dirtyResourceDeltas;

        public bool TryGetResource(long placementId, out OpenWorldResourceOverlayState state)
        {
            return _resourceStates.TryGetValue(placementId, out state);
        }

        public bool ApplyResourceState(OpenWorldResourceOverlayState state)
        {
            if (state.PlacementId == 0)
                throw new ArgumentOutOfRangeException(nameof(state), state.PlacementId, "Overlay placement id must be non-zero.");

            if (_resourceStates.TryGetValue(state.PlacementId, out var current) && current.Equals(state))
                return false;

            var basisRevision = Revision;
            Revision++;
            _resourceStates[state.PlacementId] = state;

            var delta = new OpenWorldResourceOverlayDelta
            {
                PlacementId = state.PlacementId,
                RemainingAmount = state.RemainingAmount,
                Flags = state.Flags,
                RespawnTick = state.RespawnTick
            };
            _dirtyResourceDeltas.Add(delta);
            _resourceChanges.Add(new OpenWorldResourceOverlayChange(basisRevision, Revision, delta));
            return true;
        }

        public void ReplaceAbsolute(uint revision, IReadOnlyList<OpenWorldResourceOverlayState> states)
        {
            _resourceStates.Clear();
            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state.PlacementId == 0)
                    throw new ArgumentOutOfRangeException(nameof(states), state.PlacementId, "Overlay placement id must be non-zero.");

                _resourceStates[state.PlacementId] = state;
            }

            Revision = revision;
            _dirtyResourceDeltas.Clear();
            _resourceChanges.Clear();
        }

        public void ApplyResourceDeltas(
            uint basisRevision,
            uint revision,
            IReadOnlyList<OpenWorldResourceOverlayDelta> deltas)
        {
            if (Revision != basisRevision)
                throw new InvalidOperationException($"Cannot apply overlay delta for chunk {ChunkId}: local revision {Revision}, basis {basisRevision}.");
            if (revision < basisRevision)
                throw new ArgumentOutOfRangeException(nameof(revision), revision, "Overlay revision cannot move backwards.");

            for (var i = 0; i < deltas.Count; i++)
            {
                var delta = deltas[i];
                if (delta.PlacementId == 0)
                    throw new ArgumentOutOfRangeException(nameof(deltas), delta.PlacementId, "Overlay placement id must be non-zero.");

                _resourceStates.TryGetValue(delta.PlacementId, out var state);
                state.PlacementId = delta.PlacementId;
                state.RemainingAmount = delta.RemainingAmount;
                state.Flags = delta.Flags;
                state.RespawnTick = delta.RespawnTick;
                _resourceStates[delta.PlacementId] = state;
            }

            Revision = revision;
            _dirtyResourceDeltas.Clear();
            _resourceChanges.Clear();
        }

        public void CopyResourceStates(List<OpenWorldResourceOverlayState> output)
        {
            foreach (var pair in _resourceStates)
                output.Add(pair.Value);

            output.Sort((left, right) => left.PlacementId.CompareTo(right.PlacementId));
        }

        public bool CopyResourceDeltasSince(uint basisRevision, List<OpenWorldResourceOverlayDelta> output)
        {
            if (basisRevision > Revision)
                return false;

            for (var i = 0; i < _resourceChanges.Count; i++)
            {
                var change = _resourceChanges[i];
                if (change.Revision > basisRevision)
                    output.Add(change.Delta);
            }

            output.Sort((left, right) => left.PlacementId.CompareTo(right.PlacementId));
            return true;
        }

        public void ClearDirty()
        {
            _dirtyResourceDeltas.Clear();
        }
    }
}
