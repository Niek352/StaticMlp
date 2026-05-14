using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct OpenWorldTerrainDebugChunk
    {
        public OpenWorldTerrainDebugChunk(
            WorldChunkId chunkId,
            int lod,
            bool hasCollision,
            Vector3 worldOrigin)
        {
            ChunkId = chunkId;
            Lod = lod;
            HasCollision = hasCollision;
            WorldOrigin = worldOrigin;
        }

        public readonly WorldChunkId ChunkId;
        public readonly int Lod;
        public readonly bool HasCollision;
        public readonly Vector3 WorldOrigin;
    }
}
