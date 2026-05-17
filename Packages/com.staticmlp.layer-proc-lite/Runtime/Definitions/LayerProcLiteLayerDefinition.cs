using System;

namespace StaticMlp.LayerProcLite
{
    public sealed class LayerProcLiteLayerDefinition
    {
        public static readonly LayerProcLiteDependency[] NoDependencies = Array.Empty<LayerProcLiteDependency>();

        public readonly LayerProcLiteLayerId LayerId;
        public readonly float ChunkWorldSize;
        public readonly int LevelCount;
        public readonly LayerProcLiteDependency[] Dependencies;
        public readonly ILayerProcLiteLayerScheduler Scheduler;

        public LayerProcLiteLayerDefinition(
            LayerProcLiteLayerId layerId,
            float chunkWorldSize,
            ILayerProcLiteLayerScheduler scheduler,
            params LayerProcLiteDependency[] dependencies)
            : this(layerId, chunkWorldSize, 1, scheduler, dependencies)
        {
        }

        public LayerProcLiteLayerDefinition(
            LayerProcLiteLayerId layerId,
            float chunkWorldSize,
            int levelCount,
            ILayerProcLiteLayerScheduler scheduler,
            params LayerProcLiteDependency[] dependencies)
        {
            if (chunkWorldSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "Layer chunk world size must be positive.");
            if (levelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(levelCount), levelCount, "Layer level count must be positive.");

            LayerId = layerId;
            ChunkWorldSize = chunkWorldSize;
            LevelCount = levelCount;
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            Dependencies = dependencies ?? NoDependencies;
        }
    }
}
