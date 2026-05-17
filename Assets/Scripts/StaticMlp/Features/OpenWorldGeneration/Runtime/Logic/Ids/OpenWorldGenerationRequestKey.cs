using System;
using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct OpenWorldGenerationRequestKey : IEquatable<OpenWorldGenerationRequestKey>
    {
        public readonly LayerProcLiteChunkId ChunkId;
        public readonly int Lod;
        public readonly LayerProcLiteLayerMask Layers;
        public readonly GenerationOutputMask Outputs;
        public readonly uint SettingsHash;

        public OpenWorldGenerationRequestKey(
            LayerProcLiteChunkId chunkId,
            int lod,
            LayerProcLiteLayerMask layers,
            GenerationOutputMask outputs,
            uint settingsHash)
        {
            ChunkId = chunkId;
            Lod = lod;
            Layers = layers;
            Outputs = outputs;
            SettingsHash = settingsHash;
        }

        public bool Equals(OpenWorldGenerationRequestKey other)
        {
            return ChunkId == other.ChunkId
                   && Lod == other.Lod
                   && Layers == other.Layers
                   && Outputs == other.Outputs
                   && SettingsHash == other.SettingsHash;
        }

        public override bool Equals(object obj)
        {
            return obj is OpenWorldGenerationRequestKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = ChunkId.GetHashCode();
                hash = (hash * 397) ^ Lod;
                hash = (hash * 397) ^ Layers.GetHashCode();
                hash = (hash * 397) ^ Outputs.GetHashCode();
                hash = (hash * 397) ^ (int)SettingsHash;
                return hash;
            }
        }
    }
}
