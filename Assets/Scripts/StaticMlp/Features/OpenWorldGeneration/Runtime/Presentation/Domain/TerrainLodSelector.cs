using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class TerrainLodSelector
    {
        public static int SelectLod(WorldChunkId chunkId, WorldChunkId focusChunk)
        {
            var distance = Math.Max(Math.Abs(chunkId.X - focusChunk.X), Math.Abs(chunkId.Z - focusChunk.Z));
            if (distance <= 1)
                return 0;
            if (distance <= 2)
                return 1;
            if (distance <= 4)
                return 2;
            return 3;
        }
    }
}
