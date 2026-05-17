using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldGenerationLayerCatalog
    {
        public const int HEIGHTMAP_RESOLUTION = 128;
        public const int SURFACE_HEIGHT_PADDING_SAMPLES = 1;

        public static LayerProcLiteRuntime CreateRuntime(OpenWorldChunkGenerationRuntime config)
        {
            var heightPaddingWorld = config.ChunkWorldSize / (HEIGHTMAP_RESOLUTION - 1);

            return new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    OpenWorldGenerationLayerIds.Height,
                    config.ChunkWorldSize,
                    new OpenWorldHeightLayerScheduler(
                        HEIGHTMAP_RESOLUTION,
                        SURFACE_HEIGHT_PADDING_SAMPLES)))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    OpenWorldGenerationLayerIds.Surface,
                    config.ChunkWorldSize,
                    new OpenWorldSurfaceLayerScheduler(
                        HEIGHTMAP_RESOLUTION),
                    new LayerProcLiteDependency(
                        OpenWorldGenerationLayerIds.Height,
                        0,
                        0,
                        SURFACE_HEIGHT_PADDING_SAMPLES,
                        heightPaddingWorld)))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    OpenWorldGenerationLayerIds.MeshData,
                    config.ChunkWorldSize,
                    new OpenWorldMeshDataLayerScheduler(
                        HEIGHTMAP_RESOLUTION),
                    new LayerProcLiteDependency(OpenWorldGenerationLayerIds.Height, 0, 0f),
                    new LayerProcLiteDependency(OpenWorldGenerationLayerIds.Surface, 0, 0f)))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    OpenWorldGenerationLayerIds.Placements,
                    config.ChunkWorldSize,
                    new OpenWorldPlacementLayerScheduler(),
                    new LayerProcLiteDependency(OpenWorldGenerationLayerIds.Surface, 0, 0f)));
        }

        public static LayerProcLiteLayerId[] ToOutputLayerIds(GenerationOutputMask outputs)
        {
            var count = 0;
            if (HasMeshOutput(outputs)) count++;
            if (outputs.HasFlag(GenerationOutputMask.Placements)) count++;

            var layers = new LayerProcLiteLayerId[count];
            var index = 0;
            if (HasMeshOutput(outputs))
                layers[index++] = OpenWorldGenerationLayerIds.MeshData;
            if (outputs.HasFlag(GenerationOutputMask.Placements))
                layers[index] = OpenWorldGenerationLayerIds.Placements;

            return layers;
        }

        public static LayerProcLiteLayerMask ToLayerMask(GenerationOutputMask outputs)
        {
            var layers = LayerProcLiteLayerMask.None;
            var layerIds = ToOutputLayerIds(outputs);
            for (var i = 0; i < layerIds.Length; i++)
                layers = layers.With(layerIds[i]);

            return layers;
        }

        private static bool HasMeshOutput(GenerationOutputMask outputs)
        {
            return (outputs & (GenerationOutputMask.VisualMesh | GenerationOutputMask.PhysicsMesh | GenerationOutputMask.NavMeshSourceMesh)) != 0;
        }
    }
}
