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
            float low = math.sin((worldX + seed * 17.13f) * 0.0065f) * math.cos((worldZ - seed * 9.71f) * 0.0065f) * 24f;
            float mid = math.sin((worldX - seed * 3.37f) * 0.021f + (worldZ + seed * 2.11f) * 0.008f) * 7f;
            float ridgeWave = math.sin((worldX + seed) * 0.018f) * math.cos((worldZ - seed) * 0.015f);
            float ridge = ridgeWave * ridgeWave * 8f;
            return low + mid + ridge - 9f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SelectBiomeId(float height, float moisture, float waterMask)
        {
            if (waterMask > 0.5f)
                return 4;
            if (height > 14f)
                return 3;
            return moisture > 0.55f ? (byte)2 : (byte)1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SelectMaterialId(float height, float waterMask)
        {
            if (waterMask > 0.5f)
                return 0;
            if (height > 16f)
                return 3;
            if (height > 8f)
                return 2;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SelectVertexColorRgba(OpenWorldNativeSurfaceSample sample)
        {
            if (sample.WaterMask > 0.5f)
                return PackRgba(50, 95, 150, 255);

            if (sample.PrimaryMaterialId == 1)
                return PackRgba(72, 122, 62, 255);
            if (sample.PrimaryMaterialId == 2)
                return PackRgba(105, 94, 74, 255);
            if (sample.PrimaryMaterialId == 3)
                return PackRgba(170, 164, 140, 255);

            return PackRgba(82, 110, 72, 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackRgba(byte r, byte g, byte b, byte a)
        {
            return ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a;
        }
    }
}
