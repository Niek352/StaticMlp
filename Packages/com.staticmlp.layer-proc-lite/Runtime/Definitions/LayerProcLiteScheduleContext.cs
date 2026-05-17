using Unity.Jobs;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteScheduleContext
    {
        public readonly LayerProcLiteChunkKey Key;
        public readonly LayerProcLiteWorldBounds Bounds;
        public readonly LayerProcLiteProviderSet Providers;
        public readonly JobHandle DependencyHandle;
        public readonly object Settings;

        public LayerProcLiteScheduleContext(
            LayerProcLiteChunkKey key,
            LayerProcLiteWorldBounds bounds,
            LayerProcLiteProviderSet providers,
            JobHandle dependencyHandle,
            object settings)
        {
            Key = key;
            Bounds = bounds;
            Providers = providers;
            DependencyHandle = dependencyHandle;
            Settings = settings;
        }
    }
}
