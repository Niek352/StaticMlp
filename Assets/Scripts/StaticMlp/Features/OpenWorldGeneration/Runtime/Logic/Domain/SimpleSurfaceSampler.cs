using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class SimpleSurfaceSampler : IHeightSampler, ISurfaceSampler
    {
        private const float WATER_LEVEL = -7f;
        private readonly float _seedOffsetX;
        private readonly float _seedOffsetZ;

        public SimpleSurfaceSampler(WorldGenerationSeed seed)
        {
            _seedOffsetX = SeedOffset(seed.Value, 92821);
            _seedOffsetZ = SeedOffset(seed.Value, 51787);
        }

        public float SampleHeight(float worldX, float worldZ)
        {
            var baseNoise = Mathf.PerlinNoise((worldX + _seedOffsetX) * 0.0065f, (worldZ + _seedOffsetZ) * 0.0065f);
            var detailNoise = Mathf.PerlinNoise((worldX - _seedOffsetZ) * 0.021f, (worldZ + _seedOffsetX) * 0.021f);
            var ridge = Mathf.Sin((worldX + _seedOffsetX) * 0.018f) * Mathf.Cos((worldZ - _seedOffsetZ) * 0.015f);
            return (baseNoise - 0.48f) * 42f + (detailNoise - 0.5f) * 9f + ridge * 4f;
        }

        public SurfaceSample Sample(float worldX, float worldZ)
        {
            var height = SampleHeight(worldX, worldZ);
            var normal = SampleNormal(worldX, worldZ);
            var moisture = Mathf.PerlinNoise((worldX + _seedOffsetZ) * 0.004f, (worldZ - _seedOffsetX) * 0.004f);
            var waterMask = height <= WATER_LEVEL ? 1f : 0f;
            var biomeId = SelectBiomeId(height, moisture, waterMask);
            var materialId = SelectMaterialId(height, waterMask);
            var wetness = Mathf.Clamp01((WATER_LEVEL + 3f - height) / 6f + moisture * 0.35f);

            return new SurfaceSample(
                height,
                normal,
                biomeId,
                materialId,
                0f,
                waterMask,
                wetness);
        }

        private Vector3 SampleNormal(float worldX, float worldZ)
        {
            const float normalStep = 1f;
            var left = SampleHeight(worldX - normalStep, worldZ);
            var right = SampleHeight(worldX + normalStep, worldZ);
            var down = SampleHeight(worldX, worldZ - normalStep);
            var up = SampleHeight(worldX, worldZ + normalStep);
            return new Vector3(left - right, normalStep * 2f, down - up).normalized;
        }

        private static byte SelectBiomeId(float height, float moisture, float waterMask)
        {
            if (waterMask > 0.5f)
                return 4;
            if (height > 14f)
                return 3;
            return moisture > 0.55f ? (byte)2 : (byte)1;
        }

        private static byte SelectMaterialId(float height, float waterMask)
        {
            if (waterMask > 0.5f)
                return 0;
            if (height > 16f)
                return 3;
            if (height > 8f)
                return 2;
            return 1;
        }

        private static float SeedOffset(int seed, int salt)
        {
            unchecked
            {
                var hash = seed;
                hash = (hash * 397) ^ salt;
                hash ^= hash << 13;
                hash ^= hash >> 17;
                hash ^= hash << 5;
                return (hash & 0xFFFF) * 0.37f;
            }
        }
    }
}
