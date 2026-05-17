using StaticMlp.LayerProcLite;
using Unity.Collections;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldMeshChunkData : ILayerProcLiteChunkData
    {
        public readonly float ChunkWorldSize;
        public readonly NativeArray<float3> Vertices;
        public readonly NativeArray<float3> Normals;
        public readonly NativeArray<float4> Tangents;
        public readonly NativeArray<float2> Uvs;
        public readonly NativeArray<uint> ColorsRgba;
        public readonly NativeArray<int> Triangles;
        public readonly NativeArray<float> OutMinY;
        public readonly NativeArray<float> OutMaxY;

        public OpenWorldMeshChunkData(
            float chunkWorldSize,
            NativeArray<float3> vertices,
            NativeArray<float3> normals,
            NativeArray<float4> tangents,
            NativeArray<float2> uvs,
            NativeArray<uint> colorsRgba,
            NativeArray<int> triangles,
            NativeArray<float> outMinY,
            NativeArray<float> outMaxY)
        {
            ChunkWorldSize = chunkWorldSize;
            Vertices = vertices;
            Normals = normals;
            Tangents = tangents;
            Uvs = uvs;
            ColorsRgba = colorsRgba;
            Triangles = triangles;
            OutMinY = outMinY;
            OutMaxY = outMaxY;
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uvs.Dispose();
            ColorsRgba.Dispose();
            Triangles.Dispose();
            OutMinY.Dispose();
            OutMaxY.Dispose();
        }
    }
}
