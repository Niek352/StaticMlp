using StaticMlp.LayerProcLite;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace StaticMlp.Features.OpenWorldGeneration.Jobs
{
    [BurstCompile]
    public struct OpenWorldHeightmapGenerationJob : IJobParallelFor
    {
        public LayerProcLitePlanStep Step;
        public LayerProcLiteChunkId ChunkId;
        public uint WorldSeed;
        public float ChunkWorldSize;
        public int OutputResolution;

        [WriteOnly] public NativeArray<float> PaddedHeights;

        public void Execute(int index)
        {
            int inputResolution = OutputResolution + Step.Window.PaddingSamples * 2;
            var input = LayerProcLiteGrid.FromIndex(index, inputResolution);
            int localX = input.x - Step.Window.PaddingSamples;
            int localZ = input.y - Step.Window.PaddingSamples;
            var worldPos = LayerProcLiteGrid.SampleWorldPosition(ChunkId, localX, localZ, ChunkWorldSize, OutputResolution);

            PaddedHeights[index] = OpenWorldSurfaceRules.SampleHeight(WorldSeed, worldPos.x, worldPos.y);
        }
    }
}
