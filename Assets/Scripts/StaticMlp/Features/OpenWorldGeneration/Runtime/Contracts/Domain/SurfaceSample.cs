using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct SurfaceSample
    {
        public readonly float Height;
        public readonly Vector3 Normal;
        public readonly byte BiomeId;
        public readonly byte PrimaryMaterialId;
        public readonly float RoadMask;
        public readonly float WaterMask;
        public readonly float Wetness;

        public SurfaceSample(
            float height,
            Vector3 normal,
            byte biomeId,
            byte primaryMaterialId,
            float roadMask,
            float waterMask,
            float wetness)
        {
            Height = height;
            Normal = normal;
            BiomeId = biomeId;
            PrimaryMaterialId = primaryMaterialId;
            RoadMask = roadMask;
            WaterMask = waterMask;
            Wetness = wetness;
        }
    }
}
