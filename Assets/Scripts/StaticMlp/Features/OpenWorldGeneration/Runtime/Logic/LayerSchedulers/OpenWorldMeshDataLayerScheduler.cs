using System;
using StaticMlp.Features.OpenWorldGeneration.Jobs;
using StaticMlp.LayerProcLite;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldMeshDataLayerScheduler : ILayerProcLiteLayerScheduler
    {
        private readonly int _outputResolution;

        public OpenWorldMeshDataLayerScheduler(int outputResolution)
        {
            _outputResolution = outputResolution;
        }

        public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
        {
            var settings = OpenWorldLayerGenerationSettings.FromContext(context);
            if (settings.Lod != context.Key.Variant)
                throw new InvalidOperationException("OpenWorld mesh layer key variant must match the request LOD.");

            var height = context.Providers.GetSingleOverlapping<OpenWorldHeightChunkData>(
                OpenWorldGenerationLayerIds.Height,
                0,
                context.Bounds);
            var surface = context.Providers.GetSingleOverlapping<OpenWorldSurfaceChunkData>(
                OpenWorldGenerationLayerIds.Surface,
                0,
                context.Bounds);
            var quads = settings.BaseQuadCount >> context.Key.Variant;
            var gridVerts = (quads + 1) * (quads + 1);
            var skirtVerts = settings.AddSkirts ? 4 * (quads + 1) : 0;
            var totalVerts = gridVerts + skirtVerts;
            var gridTris = quads * quads * 6;
            var skirtTris = settings.AddSkirts ? 4 * quads * 6 : 0;
            var totalTris = gridTris + skirtTris;
            var vertices = new NativeArray<float3>(totalVerts, Allocator.Persistent);
            var normals = new NativeArray<float3>(totalVerts, Allocator.Persistent);
            var tangents = new NativeArray<float4>(totalVerts, Allocator.Persistent);
            var uvs = new NativeArray<float2>(totalVerts, Allocator.Persistent);
            var colors = new NativeArray<uint>(totalVerts, Allocator.Persistent);
            var triangles = new NativeArray<int>(totalTris, Allocator.Persistent);
            var outMinY = new NativeArray<float>(1, Allocator.Persistent);
            var outMaxY = new NativeArray<float>(1, Allocator.Persistent);
            var data = new OpenWorldMeshChunkData(
                settings.ChunkWorldSize,
                vertices,
                normals,
                tangents,
                uvs,
                colors,
                triangles,
                outMinY,
                outMaxY);
            var dependencyMask = LayerProcLiteLayerMask.From(OpenWorldGenerationLayerIds.Height)
                .With(OpenWorldGenerationLayerIds.Surface);
            var step = new LayerProcLitePlanStep(context.Key.LayerId, dependencyMask, LayerProcLiteWindow.None);
            var handle = new OpenWorldTerrainMeshGenerationJob
            {
                Step = step,
                ChunkWorldSize = settings.ChunkWorldSize,
                Quads = quads,
                AddSkirts = settings.AddSkirts,
                SkirtDepth = settings.SkirtDepth,
                OutputResolution = _outputResolution,
                HeightLayout = height.Layout,
                PaddedHeights = height.PaddedHeights.AsReadOnly(),
                Surfaces = surface.Surfaces.AsReadOnly(),
                MeshBuffers = new LayerProcLiteNativeMeshBuffers(
                    vertices,
                    normals,
                    tangents,
                    uvs,
                    colors,
                    triangles),
                OutMinY = outMinY,
                OutMaxY = outMaxY
            }.Schedule(context.DependencyHandle);

            return new LayerProcLiteScheduleResult(handle, data);
        }
    }
}
