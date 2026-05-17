using System;
using Unity.Collections;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public struct OpenWorldNativeMeshBuffers
    {
        public NativeArray<float3> Vertices;
        public NativeArray<float3> Normals;
        public NativeArray<float4> Tangents;
        public NativeArray<float2> Uvs;
        public NativeArray<uint> VertexColorsRgba;
        public NativeArray<int> Triangles;

        public OpenWorldNativeMeshBuffers(
            NativeArray<float3> vertices,
            NativeArray<float3> normals,
            NativeArray<float4> tangents,
            NativeArray<float2> uvs,
            NativeArray<uint> vertexColorsRgba,
            NativeArray<int> triangles)
        {
            if (!vertices.IsCreated
                || !normals.IsCreated
                || !tangents.IsCreated
                || !uvs.IsCreated
                || !vertexColorsRgba.IsCreated
                || !triangles.IsCreated)
            {
                throw new InvalidOperationException("Native mesh buffers must be created.");
            }

            if (normals.Length != vertices.Length
                || tangents.Length != vertices.Length
                || uvs.Length != vertices.Length
                || vertexColorsRgba.Length != vertices.Length)
            {
                throw new ArgumentException("Native mesh vertex attribute buffers must have the same length.");
            }

            Vertices = vertices;
            Normals = normals;
            Tangents = tangents;
            Uvs = uvs;
            VertexColorsRgba = vertexColorsRgba;
            Triangles = triangles;
        }

        public int VertexCount => Vertices.Length;
        public int TriangleIndexCount => Triangles.Length;
    }
}
