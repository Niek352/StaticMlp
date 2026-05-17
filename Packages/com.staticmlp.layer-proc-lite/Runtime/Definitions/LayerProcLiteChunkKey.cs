using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteChunkKey : IEquatable<LayerProcLiteChunkKey>
    {
        public readonly LayerProcLiteLayerId LayerId;
        public readonly int Level;
        public readonly LayerProcLiteChunkId ChunkId;
        public readonly int Variant;
        public readonly uint SettingsHash;

        public LayerProcLiteChunkKey(
            LayerProcLiteLayerId layerId,
            int level,
            LayerProcLiteChunkId chunkId,
            int variant = 0,
            uint settingsHash = 0)
        {
            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Chunk level must be non-negative.");

            LayerId = layerId;
            Level = level;
            ChunkId = chunkId;
            Variant = variant;
            SettingsHash = settingsHash;
        }

        public bool Equals(LayerProcLiteChunkKey other)
        {
            return LayerId == other.LayerId
                   && Level == other.Level
                   && ChunkId == other.ChunkId
                   && Variant == other.Variant
                   && SettingsHash == other.SettingsHash;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteChunkKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = LayerId.GetHashCode();
                hash = (hash * 397) ^ Level;
                hash = (hash * 397) ^ ChunkId.GetHashCode();
                hash = (hash * 397) ^ Variant;
                hash = (hash * 397) ^ (int)SettingsHash;
                return hash;
            }
        }

        public static bool operator ==(LayerProcLiteChunkKey left, LayerProcLiteChunkKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerProcLiteChunkKey left, LayerProcLiteChunkKey right)
        {
            return !left.Equals(right);
        }
    }
}
