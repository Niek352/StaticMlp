using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class SimpleSurfaceSampler : IHeightSampler, ISurfaceSampler
    {
        private readonly float _seedOffsetX;
        private readonly float _seedOffsetZ;

        public SimpleSurfaceSampler(WorldGenerationSeed seed)
        {
            _seedOffsetX = SeedOffset(seed.Value, OpenWorldGenerationConfig.SURFACE_SEED_OFFSET_X_SALT);
            _seedOffsetZ = SeedOffset(seed.Value, OpenWorldGenerationConfig.SURFACE_SEED_OFFSET_Z_SALT);
        }

        public float SampleHeight(float worldX, float worldZ)
        {
            var baseNoise = Mathf.PerlinNoise(
                (worldX + _seedOffsetX) * OpenWorldGenerationConfig.SURFACE_BASE_NOISE_SCALE,
                (worldZ + _seedOffsetZ) * OpenWorldGenerationConfig.SURFACE_BASE_NOISE_SCALE);
            var detailNoise = Mathf.PerlinNoise(
                (worldX - _seedOffsetZ) * OpenWorldGenerationConfig.SURFACE_DETAIL_NOISE_SCALE,
                (worldZ + _seedOffsetX) * OpenWorldGenerationConfig.SURFACE_DETAIL_NOISE_SCALE);
            var ridge = Mathf.Sin((worldX + _seedOffsetX) * OpenWorldGenerationConfig.SURFACE_RIDGE_SCALE_X)
                        * Mathf.Cos((worldZ - _seedOffsetZ) * OpenWorldGenerationConfig.SURFACE_RIDGE_SCALE_Z);
            return (baseNoise - OpenWorldGenerationConfig.SURFACE_BASE_NOISE_BIAS) * OpenWorldGenerationConfig.SURFACE_BASE_NOISE_AMPLITUDE
                   + (detailNoise - OpenWorldGenerationConfig.SURFACE_DETAIL_NOISE_BIAS) * OpenWorldGenerationConfig.SURFACE_DETAIL_NOISE_AMPLITUDE
                   + ridge * OpenWorldGenerationConfig.SURFACE_RIDGE_AMPLITUDE;
        }

        public SurfaceSample Sample(float worldX, float worldZ)
        {
            var height = SampleHeight(worldX, worldZ);
            var normal = SampleNormal(worldX, worldZ);
            var moisture = Mathf.PerlinNoise(
                (worldX + _seedOffsetZ) * OpenWorldGenerationConfig.SURFACE_MOISTURE_NOISE_SCALE,
                (worldZ - _seedOffsetX) * OpenWorldGenerationConfig.SURFACE_MOISTURE_NOISE_SCALE);
            var waterMask = height <= OpenWorldGenerationConfig.WATER_LEVEL ? 1f : 0f;
            var biomeId = SelectBiomeId(height, moisture, waterMask);
            var materialId = SelectMaterialId(height, waterMask);
            var wetness = Mathf.Clamp01(
                (OpenWorldGenerationConfig.WATER_LEVEL + OpenWorldGenerationConfig.WETNESS_HEIGHT_OFFSET - height)
                / OpenWorldGenerationConfig.WETNESS_HEIGHT_RANGE
                + moisture * OpenWorldGenerationConfig.WETNESS_MOISTURE_WEIGHT);

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
            const float normalStep = OpenWorldGenerationConfig.SURFACE_NORMAL_SAMPLE_STEP;
            var left = SampleHeight(worldX - normalStep, worldZ);
            var right = SampleHeight(worldX + normalStep, worldZ);
            var down = SampleHeight(worldX, worldZ - normalStep);
            var up = SampleHeight(worldX, worldZ + normalStep);
            return new Vector3(left - right, normalStep * 2f, down - up).normalized;
        }

        private static byte SelectBiomeId(float height, float moisture, float waterMask)
        {
            if (waterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return 4;
            if (height > OpenWorldGenerationConfig.MOUNTAIN_BIOME_MIN_HEIGHT)
                return 3;
            return moisture > OpenWorldGenerationConfig.WET_BIOME_MIN_MOISTURE ? (byte)2 : (byte)1;
        }

        private static byte SelectMaterialId(float height, float waterMask)
        {
            if (waterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return 0;
            if (height > OpenWorldGenerationConfig.HIGH_ROCK_MATERIAL_MIN_HEIGHT)
                return 3;
            if (height > OpenWorldGenerationConfig.ROCK_MATERIAL_MIN_HEIGHT)
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
                return (hash & 0xFFFF) * OpenWorldGenerationConfig.SURFACE_SEED_OFFSET_SCALE;
            }
        }
    }
}
