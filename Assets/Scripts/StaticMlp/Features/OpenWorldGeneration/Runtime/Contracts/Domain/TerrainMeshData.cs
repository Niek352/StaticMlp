using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class TerrainMeshData
    {
        public TerrainMeshData(
            Vector3[] vertices,
            Vector3[] normals,
            Vector4[] tangents,
            Vector2[] uvs,
            Color32[] colors,
            int[] triangles,
            Bounds bounds)
        {
            Vertices = vertices;
            Normals = normals;
            Tangents = tangents;
            Uvs = uvs;
            Colors = colors;
            Triangles = triangles;
            Bounds = bounds;
        }

        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;
        public readonly Vector4[] Tangents;
        public readonly Vector2[] Uvs;
        public readonly Color32[] Colors;
        public readonly int[] Triangles;
        public readonly Bounds Bounds;
    }
}
