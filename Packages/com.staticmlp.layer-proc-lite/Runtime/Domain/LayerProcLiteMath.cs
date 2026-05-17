using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace StaticMlp.LayerProcLite
{
    public static class LayerProcLiteMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Fade(float t)
        {
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Bilinear(float c00, float c10, float c01, float c11, float tx, float tz)
        {
            float cx0 = math.lerp(c00, c10, tx);
            float cx1 = math.lerp(c01, c11, tx);
            return math.lerp(cx0, cx1, tz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Bilinear(float3 c00, float3 c10, float3 c01, float3 c11, float tx, float tz)
        {
            float3 cx0 = math.lerp(c00, c10, tx);
            float3 cx1 = math.lerp(c01, c11, tx);
            return math.lerp(cx0, cx1, tz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ValueNoise(uint seed, int2 pos)
        {
            uint n = (uint)pos.x * 374761393u + (uint)pos.y * 668265263u + seed;
            n = (n ^ (n >> 13)) * 1274126177u;
            return (n ^ (n >> 16)) / (float)uint.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(uint seed, int2 pos)
        {
            uint n = (uint)pos.x * 374761393u + (uint)pos.y * 668265263u + seed;
            n = (n ^ (n >> 13)) * 1274126177u;
            return n ^ (n >> 16);
        }
    }
}
