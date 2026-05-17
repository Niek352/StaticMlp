using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace StaticMlp.LayerProcLite
{
    public static class LayerProcLiteGrid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 FromIndex(int index, int width)
        {
            return new int2(index % width, index / width);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIndex(int x, int z, int width)
        {
            return z * width + x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 SampleWorldPosition(
            LayerProcLiteChunkId chunkId,
            int localX,
            int localZ,
            float chunkWorldSize,
            int outputResolution)
        {
            float step = chunkWorldSize / (outputResolution - 1);
            return new float2(
                chunkId.X * chunkWorldSize + localX * step,
                chunkId.Z * chunkWorldSize + localZ * step);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SampleBilinear(
            NativeArray<float>.ReadOnly data,
            int width,
            int height,
            float x,
            float z)
        {
            int x0 = (int)math.floor(x);
            int z0 = (int)math.floor(z);
            int x1 = math.min(x0 + 1, width - 1);
            int z1 = math.min(z0 + 1, height - 1);
            float tx = x - x0;
            float tz = z - z0;

            float c00 = data[math.clamp(z0 * width + x0, 0, width * height - 1)];
            float c10 = data[math.clamp(z0 * width + x1, 0, width * height - 1)];
            float c01 = data[math.clamp(z1 * width + x0, 0, width * height - 1)];
            float c11 = data[math.clamp(z1 * width + x1, 0, width * height - 1)];

            return LayerProcLiteMath.Bilinear(c00, c10, c01, c11, tx, tz);
        }
    }
}
