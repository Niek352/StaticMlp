using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct OpenWorldTerrainDebugSnapshot
    {
        public OpenWorldTerrainDebugSnapshot(
            WorldGenerationSeed seed,
            WorldChunkBounds bounds,
            float chunkWorldSize,
            bool hasFocusChunk,
            WorldChunkId focusChunk,
            OpenWorldTerrainDebugChunk[] chunks)
        {
            Seed = seed;
            Bounds = bounds;
            ChunkWorldSize = chunkWorldSize;
            HasFocusChunk = hasFocusChunk;
            FocusChunk = focusChunk;
            Chunks = chunks ?? throw new ArgumentNullException(nameof(chunks));
            LoadedChunkCount = chunks.Length;
        }

        public readonly WorldGenerationSeed Seed;
        public readonly WorldChunkBounds Bounds;
        public readonly float ChunkWorldSize;
        public readonly bool HasFocusChunk;
        public readonly WorldChunkId FocusChunk;
        public readonly int LoadedChunkCount;
        public readonly OpenWorldTerrainDebugChunk[] Chunks;

        public int ColliderChunkCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < Chunks.Length; i++)
                {
                    if (Chunks[i].HasCollision)
                        count++;
                }

                return count;
            }
        }

        public int CountLod(int lod)
        {
            var count = 0;
            for (var i = 0; i < Chunks.Length; i++)
            {
                if (Chunks[i].Lod == lod)
                    count++;
            }

            return count;
        }
    }
}
