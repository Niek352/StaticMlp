using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldSurfaceRules
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SampleHeight(uint worldSeed, float worldX, float worldZ)
        {
            float seed = worldSeed;
            float low = math.sin(
                            (worldX + seed * OpenWorldGenerationConfig.NATIVE_HEIGHT_LOW_SEED_X_SCALE)
                            * OpenWorldGenerationConfig.NATIVE_HEIGHT_LOW_NOISE_SCALE)
                        * math.cos(
                            (worldZ - seed * OpenWorldGenerationConfig.NATIVE_HEIGHT_LOW_SEED_Z_SCALE)
                            * OpenWorldGenerationConfig.NATIVE_HEIGHT_LOW_NOISE_SCALE)
                        * OpenWorldGenerationConfig.NATIVE_HEIGHT_LOW_AMPLITUDE;
            float mid = math.sin(
                            (worldX - seed * OpenWorldGenerationConfig.NATIVE_HEIGHT_MID_SEED_X_SCALE)
                            * OpenWorldGenerationConfig.NATIVE_HEIGHT_MID_NOISE_SCALE_X
                            + (worldZ + seed * OpenWorldGenerationConfig.NATIVE_HEIGHT_MID_SEED_Z_SCALE)
                            * OpenWorldGenerationConfig.NATIVE_HEIGHT_MID_NOISE_SCALE_Z)
                        * OpenWorldGenerationConfig.NATIVE_HEIGHT_MID_AMPLITUDE;
            float ridgeWave = math.sin(
                                  (worldX + seed) * OpenWorldGenerationConfig.NATIVE_HEIGHT_RIDGE_NOISE_SCALE_X)
                              * math.cos(
                                  (worldZ - seed) * OpenWorldGenerationConfig.NATIVE_HEIGHT_RIDGE_NOISE_SCALE_Z);
            float ridge = ridgeWave * ridgeWave * OpenWorldGenerationConfig.NATIVE_HEIGHT_RIDGE_AMPLITUDE;
            return low + mid + ridge + OpenWorldGenerationConfig.NATIVE_HEIGHT_OFFSET;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SelectBiomeId(float height, float moisture, float waterMask)
        {
            if (waterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return 4;
            if (height > OpenWorldGenerationConfig.MOUNTAIN_BIOME_MIN_HEIGHT)
                return 3;
            return moisture > OpenWorldGenerationConfig.WET_BIOME_MIN_MOISTURE ? (byte)2 : (byte)1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SelectMaterialId(float height, float waterMask)
        {
            if (waterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return 0;
            if (height > OpenWorldGenerationConfig.HIGH_ROCK_MATERIAL_MIN_HEIGHT)
                return 3;
            if (height > OpenWorldGenerationConfig.ROCK_MATERIAL_MIN_HEIGHT)
                return 2;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SelectVertexColorRgba(OpenWorldNativeSurfaceSample sample)
        {
            if (sample.WaterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return PackRgba(
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A);

            if (sample.PrimaryMaterialId == 1)
                return PackRgba(
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A);
            if (sample.PrimaryMaterialId == 2)
                return PackRgba(
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A);
            if (sample.PrimaryMaterialId == 3)
                return PackRgba(
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A);

            return PackRgba(
                OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_R,
                OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_G,
                OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_B,
                OpenWorldGenerationConfig.VERTEX_COLOR_A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackRgba(byte r, byte g, byte b, byte a)
        {
            return ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a;
        }
    }
}
