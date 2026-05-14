using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal readonly struct LayerProcGenWorldSettings : IEquatable<LayerProcGenWorldSettings>
    {
        public LayerProcGenWorldSettings(WorldGenerationRequest request, int chunkWorldSize)
        {
            Seed = request.Seed;
            Bounds = request.Bounds;
            ChunkWorldSize = chunkWorldSize;
            BaseQuadCount = request.BaseQuadCount;
            AddSkirts = request.AddSkirts;
            SkirtDepth = request.SkirtDepth;
        }

        public readonly WorldGenerationSeed Seed;
        public readonly WorldChunkBounds Bounds;
        public readonly int ChunkWorldSize;
        public readonly int BaseQuadCount;
        public readonly bool AddSkirts;
        public readonly float SkirtDepth;

        public bool Equals(LayerProcGenWorldSettings other)
        {
            return Seed.Equals(other.Seed)
                   && Bounds.Equals(other.Bounds)
                   && ChunkWorldSize == other.ChunkWorldSize
                   && BaseQuadCount == other.BaseQuadCount
                   && AddSkirts == other.AddSkirts
                   && Math.Abs(SkirtDepth - other.SkirtDepth) < 0.0001f;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcGenWorldSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Seed.GetHashCode();
                hash = (hash * 397) ^ Bounds.GetHashCode();
                hash = (hash * 397) ^ ChunkWorldSize;
                hash = (hash * 397) ^ BaseQuadCount;
                hash = (hash * 397) ^ AddSkirts.GetHashCode();
                hash = (hash * 397) ^ SkirtDepth.GetHashCode();
                return hash;
            }
        }
    }
}
