using StaticMlp.Features.OpenWorldGeneration.Jobs;
using StaticMlp.LayerProcLite;
using Unity.Collections;
using Unity.Jobs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldHeightLayerScheduler : ILayerProcLiteLayerScheduler
    {
        private readonly int _outputResolution;
        private readonly int _paddingSamples;

        public OpenWorldHeightLayerScheduler(
            int outputResolution,
            int paddingSamples)
        {
            _outputResolution = outputResolution;
            _paddingSamples = paddingSamples;
        }

        public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
        {
            var settings = OpenWorldLayerGenerationSettings.FromContext(context);
            var layout = new LayerProcLiteGridLayout(_outputResolution, _paddingSamples);
            var heights = new NativeArray<float>(layout.InputSampleCount, Allocator.Persistent);
            var data = new OpenWorldHeightChunkData(layout, heights);
            var step = new LayerProcLitePlanStep(
                context.Key.LayerId,
                LayerProcLiteLayerMask.None,
                new LayerProcLiteWindow(_paddingSamples, 0f));
            var handle = new OpenWorldHeightmapGenerationJob
            {
                Step = step,
                ChunkId = context.Key.ChunkId,
                WorldSeed = settings.WorldSeed,
                ChunkWorldSize = settings.ChunkWorldSize,
                OutputResolution = layout.OutputResolution,
                PaddedHeights = heights
            }.Schedule(layout.InputSampleCount, 32, context.DependencyHandle);

            return new LayerProcLiteScheduleResult(handle, data);
        }
    }
}
