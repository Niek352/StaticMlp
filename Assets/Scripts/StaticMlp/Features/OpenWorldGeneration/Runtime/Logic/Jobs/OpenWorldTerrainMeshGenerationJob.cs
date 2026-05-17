using StaticMlp.LayerProcLite;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration.Jobs
{
    [BurstCompile]
    public struct OpenWorldTerrainMeshGenerationJob : IJob
    {
        public LayerProcLitePlanStep Step;
        public float ChunkWorldSize;
        public int Quads;
        public bool AddSkirts;
        public float SkirtDepth;
        public int OutputResolution;
        public LayerProcLiteGridLayout HeightLayout;

        [ReadOnly] public NativeArray<float>.ReadOnly PaddedHeights;
        [ReadOnly] public NativeArray<OpenWorldNativeSurfaceSample>.ReadOnly Surfaces;

        public OpenWorldNativeMeshBuffers MeshBuffers;
        public NativeArray<float> OutMinY;
        public NativeArray<float> OutMaxY;

        public void Execute()
        {
            int gridVerts = (Quads + 1) * (Quads + 1);
            float step = ChunkWorldSize / Quads;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            for (int z = 0; z <= Quads; z++)
            {
                for (int x = 0; x <= Quads; x++)
                {
                    int vi = z * (Quads + 1) + x;
                    float localX = x * step;
                    float localZ = z * step;
                    float hx = x * (OutputResolution - 1) / (float)Quads;
                    float hz = z * (OutputResolution - 1) / (float)Quads;
                    float height = SampleBilinearHeight(hx, hz);
                    float3 normal = SampleBilinearNormal(hx, hz);
                    var surface = SampleNearestSurface(hx, hz);

                    MeshBuffers.Vertices[vi] = new float3(localX, height, localZ);
                    MeshBuffers.Normals[vi] = normal;
                    MeshBuffers.Tangents[vi] = new float4(1f, 0f, 0f, 1f);
                    MeshBuffers.Uvs[vi] = new float2(x / (float)Quads, z / (float)Quads);
                    MeshBuffers.VertexColorsRgba[vi] = OpenWorldSurfaceRules.SelectVertexColorRgba(surface);

                    minY = math.min(minY, height);
                    maxY = math.max(maxY, height);
                }
            }

            int ti = WriteGridTriangles();
            if (AddSkirts)
                WriteSkirts(gridVerts, ref ti, ref minY);

            OutMinY[0] = minY;
            OutMaxY[0] = maxY;
        }

        private int WriteGridTriangles()
        {
            int ti = 0;
            for (int z = 0; z < Quads; z++)
            {
                for (int x = 0; x < Quads; x++)
                {
                    int v00 = z * (Quads + 1) + x;
                    int v10 = v00 + 1;
                    int v01 = v00 + (Quads + 1);
                    int v11 = v01 + 1;

                    MeshBuffers.Triangles[ti++] = v00;
                    MeshBuffers.Triangles[ti++] = v01;
                    MeshBuffers.Triangles[ti++] = v10;
                    MeshBuffers.Triangles[ti++] = v10;
                    MeshBuffers.Triangles[ti++] = v01;
                    MeshBuffers.Triangles[ti++] = v11;
                }
            }

            return ti;
        }

        private void WriteSkirts(int skirtBase, ref int ti, ref float minY)
        {
            int leftBase = skirtBase;
            for (int z = 0; z <= Quads; z++)
            {
                int src = z * (Quads + 1);
                CopySkirtVertex(leftBase + z, src, ref minY);
            }
            for (int z = 0; z < Quads; z++)
            {
                int o0 = z * (Quads + 1);
                int o1 = (z + 1) * (Quads + 1);
                int s0 = leftBase + z;
                int s1 = leftBase + z + 1;
                MeshBuffers.Triangles[ti++] = o0;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = s1;
            }

            int rightBase = leftBase + Quads + 1;
            for (int z = 0; z <= Quads; z++)
            {
                int src = z * (Quads + 1) + Quads;
                CopySkirtVertex(rightBase + z, src, ref minY);
            }
            for (int z = 0; z < Quads; z++)
            {
                int o0 = z * (Quads + 1) + Quads;
                int o1 = (z + 1) * (Quads + 1) + Quads;
                int s0 = rightBase + z;
                int s1 = rightBase + z + 1;
                MeshBuffers.Triangles[ti++] = o0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s1;
                MeshBuffers.Triangles[ti++] = s0;
            }

            int bottomBase = rightBase + Quads + 1;
            for (int x = 0; x <= Quads; x++)
                CopySkirtVertex(bottomBase + x, x, ref minY);
            for (int x = 0; x < Quads; x++)
            {
                int o0 = x;
                int o1 = x + 1;
                int s0 = bottomBase + x;
                int s1 = bottomBase + x + 1;
                MeshBuffers.Triangles[ti++] = o0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s1;
                MeshBuffers.Triangles[ti++] = s0;
            }

            int topBase = bottomBase + Quads + 1;
            for (int x = 0; x <= Quads; x++)
            {
                int src = Quads * (Quads + 1) + x;
                CopySkirtVertex(topBase + x, src, ref minY);
            }
            for (int x = 0; x < Quads; x++)
            {
                int o0 = Quads * (Quads + 1) + x;
                int o1 = Quads * (Quads + 1) + x + 1;
                int s0 = topBase + x;
                int s1 = topBase + x + 1;
                MeshBuffers.Triangles[ti++] = o0;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = o1;
                MeshBuffers.Triangles[ti++] = s0;
                MeshBuffers.Triangles[ti++] = s1;
            }
        }

        private void CopySkirtVertex(int dst, int src, ref float minY)
        {
            MeshBuffers.Vertices[dst] = new float3(MeshBuffers.Vertices[src].x, MeshBuffers.Vertices[src].y - SkirtDepth, MeshBuffers.Vertices[src].z);
            MeshBuffers.Normals[dst] = MeshBuffers.Normals[src];
            MeshBuffers.Tangents[dst] = MeshBuffers.Tangents[src];
            MeshBuffers.Uvs[dst] = MeshBuffers.Uvs[src];
            MeshBuffers.VertexColorsRgba[dst] = MeshBuffers.VertexColorsRgba[src];
            minY = math.min(minY, MeshBuffers.Vertices[dst].y);
        }

        private float SampleBilinearHeight(float hx, float hz)
        {
            return LayerProcLiteGrid.SampleBilinear(
                PaddedHeights,
                HeightLayout.InputResolution,
                HeightLayout.InputResolution,
                hx + HeightLayout.PaddingSamples,
                hz + HeightLayout.PaddingSamples);
        }

        private float3 SampleBilinearNormal(float hx, float hz)
        {
            int x0 = (int)math.floor(hx);
            int z0 = (int)math.floor(hz);
            int x1 = math.min(x0 + 1, OutputResolution - 1);
            int z1 = math.min(z0 + 1, OutputResolution - 1);
            float tx = hx - x0;
            float tz = hz - z0;

            int i00 = math.clamp(z0 * OutputResolution + x0, 0, OutputResolution * OutputResolution - 1);
            int i10 = math.clamp(z0 * OutputResolution + x1, 0, OutputResolution * OutputResolution - 1);
            int i01 = math.clamp(z1 * OutputResolution + x0, 0, OutputResolution * OutputResolution - 1);
            int i11 = math.clamp(z1 * OutputResolution + x1, 0, OutputResolution * OutputResolution - 1);

            float3 n00 = Surfaces[i00].Normal;
            float3 n10 = Surfaces[i10].Normal;
            float3 n01 = Surfaces[i01].Normal;
            float3 n11 = Surfaces[i11].Normal;

            return math.normalize(LayerProcLiteMath.Bilinear(n00, n10, n01, n11, tx, tz));
        }

        private OpenWorldNativeSurfaceSample SampleNearestSurface(float hx, float hz)
        {
            int ix = (int)math.clamp(math.round(hx), 0, OutputResolution - 1);
            int iz = (int)math.clamp(math.round(hz), 0, OutputResolution - 1);
            return Surfaces[iz * OutputResolution + ix];
        }
    }
}
