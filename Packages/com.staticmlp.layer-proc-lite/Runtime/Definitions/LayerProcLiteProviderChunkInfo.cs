using Unity.Jobs;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteProviderChunkInfo
    {
        public readonly LayerProcLiteChunkKey Key;
        public readonly LayerProcLiteWorldBounds Bounds;
        public readonly ILayerProcLiteChunkData Data;
        public readonly JobHandle Handle;

        public LayerProcLiteProviderChunkInfo(
            LayerProcLiteChunkKey key,
            LayerProcLiteWorldBounds bounds,
            ILayerProcLiteChunkData data,
            JobHandle handle)
        {
            Key = key;
            Bounds = bounds;
            Data = data;
            Handle = handle;
        }
    }
}
