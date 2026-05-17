using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteGenerationRequestKey : IEquatable<LayerProcLiteGenerationRequestKey>
    {
        public readonly LayerProcLiteChunkId ChunkId;
        public readonly int Lod;
        public readonly LayerProcLiteLayerMask Layers;
        public readonly uint SettingsHash;

        public LayerProcLiteGenerationRequestKey(
            LayerProcLiteChunkId chunkId,
            int lod,
            LayerProcLiteLayerMask layers,
            uint settingsHash)
        {
            ChunkId = chunkId;
            Lod = lod;
            Layers = layers;
            SettingsHash = settingsHash;
        }

        public bool Equals(LayerProcLiteGenerationRequestKey other)
        {
            return ChunkId == other.ChunkId
                   && Lod == other.Lod
                   && Layers == other.Layers
                   && SettingsHash == other.SettingsHash;
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteGenerationRequestKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = ChunkId.GetHashCode();
                hash = (hash * 397) ^ Lod;
                hash = (hash * 397) ^ Layers.GetHashCode();
                hash = (hash * 397) ^ (int)SettingsHash;
                return hash;
            }
        }
    }
}
