using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [Obsolete("Use OpenWorldChunkGenerationSystem instead")]
    public static class TerrainMeshBuilder
    {
        private static readonly Vector4 DEFAULT_TANGENT = new(1f, 0f, 0f, 1f);

        public static TerrainMeshData Build(TerrainMeshBuildRequest request, ISurfaceSampler surfaceSampler)
        {
            if (surfaceSampler == null)
                throw new ArgumentNullException(nameof(surfaceSampler));
            if (request.ChunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(request.ChunkWorldSize), request.ChunkWorldSize, "Chunk world size must be positive.");
            if (request.BaseQuadCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.BaseQuadCount), request.BaseQuadCount, "Base quad count must be positive.");
            if (request.Lod < 0)
                throw new ArgumentOutOfRangeException(nameof(request.Lod), request.Lod, "LOD must be non-negative.");

            var quads = request.BaseQuadCount >> request.Lod;
            if (quads <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.Lod), request.Lod, "LOD is too high for the base quad count.");

            var vertices = new List<Vector3>((quads + 1) * (quads + 1));
            var normals = new List<Vector3>((quads + 1) * (quads + 1));
            var tangents = new List<Vector4>((quads + 1) * (quads + 1));
            var uvs = new List<Vector2>((quads + 1) * (quads + 1));
            var colors = new List<Color32>((quads + 1) * (quads + 1));
            var triangles = new List<int>(quads * quads * 6);
            var chunkOriginX = request.ChunkId.X * request.ChunkWorldSize;
            var chunkOriginZ = request.ChunkId.Z * request.ChunkWorldSize;
            var step = request.ChunkWorldSize / quads;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;

            for (var z = 0; z <= quads; z++)
            {
                for (var x = 0; x <= quads; x++)
                {
                    var localX = x * step;
                    var localZ = z * step;
                    var sample = surfaceSampler.Sample(chunkOriginX + localX, chunkOriginZ + localZ);
                    vertices.Add(new Vector3(localX, sample.Height, localZ));
                    normals.Add(sample.Normal);
                    tangents.Add(DEFAULT_TANGENT);
                    uvs.Add(new Vector2((float)x / quads, (float)z / quads));
                    colors.Add(ToVertexColor(sample));
                    minY = Math.Min(minY, sample.Height);
                    maxY = Math.Max(maxY, sample.Height);
                }
            }

            for (var z = 0; z < quads; z++)
            {
                for (var x = 0; x < quads; x++)
                {
                    var v00 = GridIndex(x, z, quads);
                    var v10 = GridIndex(x + 1, z, quads);
                    var v01 = GridIndex(x, z + 1, quads);
                    var v11 = GridIndex(x + 1, z + 1, quads);

                    triangles.Add(v00);
                    triangles.Add(v01);
                    triangles.Add(v10);
                    triangles.Add(v10);
                    triangles.Add(v01);
                    triangles.Add(v11);
                }
            }

            if (request.AddSkirts)
                AddSkirts(request, quads, vertices, normals, tangents, uvs, colors, triangles, ref minY);

            var boundsHeight = maxY - minY;
            var bounds = new Bounds(
                new Vector3(request.ChunkWorldSize * 0.5f, minY + boundsHeight * 0.5f, request.ChunkWorldSize * 0.5f),
                new Vector3(request.ChunkWorldSize, boundsHeight, request.ChunkWorldSize));

            return new TerrainMeshData(
                vertices.ToArray(),
                normals.ToArray(),
                tangents.ToArray(),
                uvs.ToArray(),
                colors.ToArray(),
                triangles.ToArray(),
                bounds);
        }

        private static void AddSkirts(
            TerrainMeshBuildRequest request,
            int quads,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            ref float minY)
        {
            if (request.SkirtDepth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(request.SkirtDepth), request.SkirtDepth, "Skirt depth must be positive.");

            AddLeftSkirt(request, quads, vertices, normals, tangents, uvs, colors, triangles, ref minY);
            AddRightSkirt(request, quads, vertices, normals, tangents, uvs, colors, triangles, ref minY);
            AddBottomSkirt(request, quads, vertices, normals, tangents, uvs, colors, triangles, ref minY);
            AddTopSkirt(request, quads, vertices, normals, tangents, uvs, colors, triangles, ref minY);
        }

        private static void AddLeftSkirt(
            TerrainMeshBuildRequest request,
            int quads,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            ref float minY)
        {
            var firstSkirtVertex = vertices.Count;
            for (var z = 0; z <= quads; z++)
                AddSkirtVertex(GridIndex(0, z, quads), request.SkirtDepth, vertices, normals, tangents, uvs, colors, ref minY);

            for (var z = 0; z < quads; z++)
            {
                var o0 = GridIndex(0, z, quads);
                var o1 = GridIndex(0, z + 1, quads);
                var s0 = firstSkirtVertex + z;
                var s1 = firstSkirtVertex + z + 1;
                triangles.Add(o0);
                triangles.Add(s0);
                triangles.Add(o1);
                triangles.Add(o1);
                triangles.Add(s0);
                triangles.Add(s1);
            }
        }

        private static void AddRightSkirt(
            TerrainMeshBuildRequest request,
            int quads,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            ref float minY)
        {
            var firstSkirtVertex = vertices.Count;
            for (var z = 0; z <= quads; z++)
                AddSkirtVertex(GridIndex(quads, z, quads), request.SkirtDepth, vertices, normals, tangents, uvs, colors, ref minY);

            for (var z = 0; z < quads; z++)
            {
                var o0 = GridIndex(quads, z, quads);
                var o1 = GridIndex(quads, z + 1, quads);
                var s0 = firstSkirtVertex + z;
                var s1 = firstSkirtVertex + z + 1;
                triangles.Add(o0);
                triangles.Add(o1);
                triangles.Add(s0);
                triangles.Add(o1);
                triangles.Add(s1);
                triangles.Add(s0);
            }
        }

        private static void AddBottomSkirt(
            TerrainMeshBuildRequest request,
            int quads,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            ref float minY)
        {
            var firstSkirtVertex = vertices.Count;
            for (var x = 0; x <= quads; x++)
                AddSkirtVertex(GridIndex(x, 0, quads), request.SkirtDepth, vertices, normals, tangents, uvs, colors, ref minY);

            for (var x = 0; x < quads; x++)
            {
                var o0 = GridIndex(x, 0, quads);
                var o1 = GridIndex(x + 1, 0, quads);
                var s0 = firstSkirtVertex + x;
                var s1 = firstSkirtVertex + x + 1;
                triangles.Add(o0);
                triangles.Add(o1);
                triangles.Add(s0);
                triangles.Add(o1);
                triangles.Add(s1);
                triangles.Add(s0);
            }
        }

        private static void AddTopSkirt(
            TerrainMeshBuildRequest request,
            int quads,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            ref float minY)
        {
            var firstSkirtVertex = vertices.Count;
            for (var x = 0; x <= quads; x++)
                AddSkirtVertex(GridIndex(x, quads, quads), request.SkirtDepth, vertices, normals, tangents, uvs, colors, ref minY);

            for (var x = 0; x < quads; x++)
            {
                var o0 = GridIndex(x, quads, quads);
                var o1 = GridIndex(x + 1, quads, quads);
                var s0 = firstSkirtVertex + x;
                var s1 = firstSkirtVertex + x + 1;
                triangles.Add(o0);
                triangles.Add(s0);
                triangles.Add(o1);
                triangles.Add(o1);
                triangles.Add(s0);
                triangles.Add(s1);
            }
        }

        private static void AddSkirtVertex(
            int sourceIndex,
            float skirtDepth,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Vector2> uvs,
            List<Color32> colors,
            ref float minY)
        {
            var vertex = vertices[sourceIndex];
            vertex.y -= skirtDepth;
            vertices.Add(vertex);
            normals.Add(normals[sourceIndex]);
            tangents.Add(tangents[sourceIndex]);
            uvs.Add(uvs[sourceIndex]);
            colors.Add(colors[sourceIndex]);
            minY = Math.Min(minY, vertex.y);
        }

        private static int GridIndex(int x, int z, int quads)
        {
            return z * (quads + 1) + x;
        }

        private static Color32 ToVertexColor(SurfaceSample sample)
        {
            if (sample.WaterMask > OpenWorldGenerationConfig.WATER_BIOME_MASK_THRESHOLD)
                return new Color32(
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.WATER_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A);

            return sample.PrimaryMaterialId switch
            {
                1 => new Color32(
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.GRASS_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A),
                2 => new Color32(
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.DIRT_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A),
                3 => new Color32(
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.ROCK_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A),
                _ => new Color32(
                    OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_R,
                    OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_G,
                    OpenWorldGenerationConfig.FALLBACK_VERTEX_COLOR_B,
                    OpenWorldGenerationConfig.VERTEX_COLOR_A)
            };
        }
    }
}
