using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldSpatialClusterIds
    {
        public const ushort FIRST_CLUSTER_ID = 1024;

        public static ushort ToClusterId(WorldChunkId chunkId, WorldChunkBounds bounds)
        {
            ValidateBoundsCapacity(bounds);
            if (!bounds.Contains(chunkId))
                throw new ArgumentOutOfRangeException(nameof(chunkId), chunkId, "Chunk is outside open world spatial bounds.");

            var width = (long)bounds.MaxX - bounds.MinX + 1;
            var index = ((long)chunkId.Z - bounds.MinZ) * width + chunkId.X - bounds.MinX;
            return checked((ushort)(FIRST_CLUSTER_ID + index));
        }

        public static WorldChunkId ToChunkId(ushort clusterId, WorldChunkBounds bounds)
        {
            ValidateBoundsCapacity(bounds);
            if (clusterId < FIRST_CLUSTER_ID)
                throw new ArgumentOutOfRangeException(nameof(clusterId), clusterId, "Cluster is not an open world spatial cluster.");

            var width = (long)bounds.MaxX - bounds.MinX + 1;
            var count = width * ((long)bounds.MaxZ - bounds.MinZ + 1);
            var index = (long)clusterId - FIRST_CLUSTER_ID;
            if (index >= count)
                throw new ArgumentOutOfRangeException(nameof(clusterId), clusterId, "Cluster is outside open world spatial bounds.");

            var z = bounds.MinZ + (int)(index / width);
            var x = bounds.MinX + (int)(index % width);
            return new WorldChunkId(x, z);
        }

        private static void ValidateBoundsCapacity(WorldChunkBounds bounds)
        {
            var width = (long)bounds.MaxX - bounds.MinX + 1;
            var height = (long)bounds.MaxZ - bounds.MinZ + 1;
            var count = width * height;
            var maxCount = ushort.MaxValue - FIRST_CLUSTER_ID + 1L;
            if (count > maxCount)
                throw new ArgumentOutOfRangeException(nameof(bounds), bounds, "Open world spatial bounds exceed available StaticEcs cluster ids.");
        }
    }
}
