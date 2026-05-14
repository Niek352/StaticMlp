using System;
using Runevision.Common;
using Runevision.LayerProcGen;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgSurfaceLayer : ChunkBasedDataLayer<LpgSurfaceLayer, LpgSurfaceChunk>
    {
        public override int chunkW => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;
        public override int chunkH => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;

        public LpgSurfaceLayer()
        {
            AddLayerDependency(new LayerDependency(LpgHeightLayer.instance, 1));
            AddLayerDependency(new LayerDependency(LpgBiomeLayer.instance, 0));
        }

        public SurfaceSample Sample(ILC requester, float worldX, float worldZ)
        {
            var bounds = new GridBounds((int)Math.Floor(worldX), (int)Math.Floor(worldZ), 1, 1);
            HandleChunksInBounds(requester, bounds, 0, _ => { });

            var heightLayer = LpgHeightLayer.instance;
            var height = heightLayer.SampleHeight(requester, worldX, worldZ);
            var normal = SampleNormal(requester, worldX, worldZ);
            var waterMask = height <= heightLayer.WaterLevel ? 1f : 0f;
            var biomeId = LpgBiomeLayer.instance.GetBiomeId(requester, worldX, worldZ, height, waterMask);
            var materialId = SelectMaterialId(height, waterMask);
            var wetness = Mathf.Clamp01((heightLayer.WaterLevel + 3f - height) / 6f);

            return new SurfaceSample(height, normal, biomeId, materialId, 0f, waterMask, wetness);
        }

        private static Vector3 SampleNormal(ILC requester, float worldX, float worldZ)
        {
            const float normalStep = 1f;
            var heightLayer = LpgHeightLayer.instance;
            var left = heightLayer.SampleHeight(requester, worldX - normalStep, worldZ);
            var right = heightLayer.SampleHeight(requester, worldX + normalStep, worldZ);
            var down = heightLayer.SampleHeight(requester, worldX, worldZ - normalStep);
            var up = heightLayer.SampleHeight(requester, worldX, worldZ + normalStep);
            return new Vector3(left - right, normalStep * 2f, down - up).normalized;
        }

        private static byte SelectMaterialId(float height, float waterMask)
        {
            if (waterMask > 0.5f)
                return 0;
            if (height > 16f)
                return 3;
            if (height > 8f)
                return 2;
            return 1;
        }
    }
}
