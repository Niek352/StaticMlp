using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public struct OpenWorldNativeSurfaceSample
    {
        public float Height;
        public float3 Normal;
        public byte BiomeId;
        public byte PrimaryMaterialId;
        public float RoadMask;
        public float WaterMask;
        public float Wetness;

        public OpenWorldNativeSurfaceSample(
            float height,
            float3 normal,
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
