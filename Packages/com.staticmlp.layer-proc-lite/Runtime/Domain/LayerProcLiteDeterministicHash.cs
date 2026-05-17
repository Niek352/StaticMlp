using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace StaticMlp.LayerProcLite
{
    public static class LayerProcLiteDeterministicHash
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(uint seed, LayerProcLiteChunkId chunkId, int stream, int index)
        {
            unchecked
            {
                uint hash = seed;
                hash ^= (uint)chunkId.X * 0x9E3779B9u;
                hash = math.rol(hash, 13);
                hash ^= (uint)chunkId.Z * 0x85EBCA6Bu;
                hash = math.rol(hash, 17);
                hash ^= (uint)stream * 0xC2B2AE35u;
                hash = math.rol(hash, 11);
                hash ^= (uint)index * 0x27D4EB2Fu;
                hash ^= hash >> 15;
                hash *= 0x2C1B3C6Du;
                hash ^= hash >> 12;
                hash *= 0x297A2D39u;
                hash ^= hash >> 15;
                return hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Unit(uint hash)
        {
            return (hash & 0x00FFFFFFu) / 16777216f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CreateStablePositiveId(uint seed, LayerProcLiteChunkId chunkId, int stream, int index)
        {
            unchecked
            {
                uint high = Hash(seed, chunkId, stream + 101, index);
                uint low = Hash(seed, chunkId, stream + 211, index);
                long value = ((long)high << 32) | low;
                value &= long.MaxValue;
                return value == 0 ? 1 : value;
            }
        }
    }
}
