using StaticMlp.LayerProcLite;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration.Jobs
{
    [BurstCompile]
    public struct OpenWorldSurfaceSamplingJob : IJobParallelFor
    {
        public LayerProcLitePlanStep Step;
        public LayerProcLiteChunkId ChunkId;
        public uint WorldSeed;
        public float ChunkWorldSize;
        public int OutputResolution;
        public LayerProcLiteGridLayout HeightLayout;
        public float WaterLevel;

        [ReadOnly] public NativeArray<float>.ReadOnly PaddedHeights;
        [WriteOnly] public NativeArray<OpenWorldNativeSurfaceSample> Surfaces;

        public void Execute(int index)
        {
            var local = LayerProcLiteGrid.FromIndex(index, OutputResolution);
            int inputX = local.x + HeightLayout.PaddingSamples;
            int inputZ = local.y + HeightLayout.PaddingSamples;
            float height = PaddedHeights[HeightLayout.ToInputIndexRaw(inputX, inputZ)];
            float3 normal = SampleNormal(inputX, inputZ);
            var worldPos = LayerProcLiteGrid.SampleWorldPosition(ChunkId, local.x, local.y, ChunkWorldSize, OutputResolution);
            float waterMask = height <= WaterLevel ? 1f : 0f;
            float moisture = LayerProcLiteMath.ValueNoise(WorldSeed, new int2((int)worldPos.x, (int)worldPos.y));
            byte biomeId = OpenWorldSurfaceRules.SelectBiomeId(height, moisture, waterMask);
            byte materialId = OpenWorldSurfaceRules.SelectMaterialId(height, waterMask);
            float wetness = math.saturate(
                (WaterLevel + OpenWorldGenerationConfig.WETNESS_HEIGHT_OFFSET - height)
                / OpenWorldGenerationConfig.WETNESS_HEIGHT_RANGE);

            Surfaces[index] = new OpenWorldNativeSurfaceSample(height, normal, biomeId, materialId, 0f, waterMask, wetness);
        }

        private float3 SampleNormal(int inputX, int inputZ)
        {
            float step = ChunkWorldSize / (OutputResolution - 1);
            int leftX = math.max(inputX - 1, 0);
            int rightX = math.min(inputX + 1, HeightLayout.InputResolution - 1);
            int downZ = math.max(inputZ - 1, 0);
            int upZ = math.min(inputZ + 1, HeightLayout.InputResolution - 1);

            float left = PaddedHeights[HeightLayout.ToInputIndexRaw(leftX, inputZ)];
            float right = PaddedHeights[HeightLayout.ToInputIndexRaw(rightX, inputZ)];
            float down = PaddedHeights[HeightLayout.ToInputIndexRaw(inputX, downZ)];
            float up = PaddedHeights[HeightLayout.ToInputIndexRaw(inputX, upZ)];

            return math.normalize(new float3(left - right, step * 2f, down - up));
        }
    }
}
