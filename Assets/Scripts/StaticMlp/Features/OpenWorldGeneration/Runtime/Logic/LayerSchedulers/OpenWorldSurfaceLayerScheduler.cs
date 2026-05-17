using StaticMlp.Features.OpenWorldGeneration.Jobs;
using StaticMlp.LayerProcLite;
using Unity.Collections;
using Unity.Jobs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldSurfaceLayerScheduler : ILayerProcLiteLayerScheduler
    {
        private readonly int _outputResolution;

        public OpenWorldSurfaceLayerScheduler(int outputResolution)
        {
            _outputResolution = outputResolution;
        }

        public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
        {
            var settings = OpenWorldLayerGenerationSettings.FromContext(context);
            var height = context.Providers.GetSingleOverlapping<OpenWorldHeightChunkData>(
                OpenWorldGenerationLayerIds.Height,
                0,
                context.Bounds);
            var surfaceLayout = new LayerProcLiteGridLayout(_outputResolution, 0);
            var surfaces = new NativeArray<OpenWorldNativeSurfaceSample>(surfaceLayout.OutputSampleCount, Allocator.Persistent);
            var data = new OpenWorldSurfaceChunkData(surfaceLayout, surfaces);
            var step = new LayerProcLitePlanStep(
                context.Key.LayerId,
                LayerProcLiteLayerMask.From(OpenWorldGenerationLayerIds.Height),
                LayerProcLiteWindow.None);
            var handle = new OpenWorldSurfaceSamplingJob
            {
                Step = step,
                ChunkId = context.Key.ChunkId,
                WorldSeed = settings.WorldSeed,
                ChunkWorldSize = settings.ChunkWorldSize,
                OutputResolution = surfaceLayout.OutputResolution,
                HeightLayout = height.Layout,
                WaterLevel = settings.WaterLevel,
                PaddedHeights = height.PaddedHeights.AsReadOnly(),
                Surfaces = surfaces
            }.Schedule(surfaceLayout.OutputSampleCount, 32, context.DependencyHandle);

            return new LayerProcLiteScheduleResult(handle, data);
        }
    }
}
