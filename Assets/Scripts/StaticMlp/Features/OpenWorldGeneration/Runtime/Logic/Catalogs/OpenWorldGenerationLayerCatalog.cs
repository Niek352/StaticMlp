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
                .RegisterLayer(CreateMeshOutputLayer(OpenWorldGenerationLayerIds.VisualMesh, config.ChunkWorldSize))
                .RegisterLayer(CreateMeshOutputLayer(OpenWorldGenerationLayerIds.PhysicsMesh, config.ChunkWorldSize))
                .RegisterLayer(CreateMeshOutputLayer(OpenWorldGenerationLayerIds.NavMeshSource, config.ChunkWorldSize))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    OpenWorldGenerationLayerIds.Placements,
                    config.ChunkWorldSize,
                    new OpenWorldPlacementLayerScheduler(),
                    new LayerProcLiteDependency(OpenWorldGenerationLayerIds.Surface, 0, 0f)));
        }

        public static LayerProcLiteLayerId[] ToOutputLayerIds(GenerationOutputMask outputs)
        {
            var count = 0;
            if (outputs.HasFlag(GenerationOutputMask.VisualMesh)) count++;
            if (outputs.HasFlag(GenerationOutputMask.PhysicsMesh)) count++;
            if (outputs.HasFlag(GenerationOutputMask.NavMeshSourceMesh)) count++;
            if (outputs.HasFlag(GenerationOutputMask.Placements)) count++;

            var layers = new LayerProcLiteLayerId[count];
            var index = 0;
            if (outputs.HasFlag(GenerationOutputMask.VisualMesh))
                layers[index++] = OpenWorldGenerationLayerIds.VisualMesh;
            if (outputs.HasFlag(GenerationOutputMask.PhysicsMesh))
                layers[index++] = OpenWorldGenerationLayerIds.PhysicsMesh;
            if (outputs.HasFlag(GenerationOutputMask.NavMeshSourceMesh))
                layers[index++] = OpenWorldGenerationLayerIds.NavMeshSource;
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

        private static LayerProcLiteLayerDefinition CreateMeshOutputLayer(
            LayerProcLiteLayerId layerId,
            float chunkWorldSize)
        {
            return new LayerProcLiteLayerDefinition(
                layerId,
                chunkWorldSize,
                new OpenWorldMeshOutputLayerScheduler(),
                new LayerProcLiteDependency(OpenWorldGenerationLayerIds.MeshData, 0, 0f));
        }
    }
}
