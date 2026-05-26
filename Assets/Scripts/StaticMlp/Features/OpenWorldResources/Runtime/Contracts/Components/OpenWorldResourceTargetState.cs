using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceTargetState : IComponent
    {
        public long PlacementId;
        public WorldChunkId ChunkId;
        public ushort KindIdValue;
        public ushort RemainingAmount;
        public OpenWorldResourceOverlayFlags Flags;
        public Vector3 WorldPosition;
    }
}
