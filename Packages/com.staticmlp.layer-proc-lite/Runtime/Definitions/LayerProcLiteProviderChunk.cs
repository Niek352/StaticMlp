using Unity.Jobs;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteProviderChunk<TData>
        where TData : class, ILayerProcLiteChunkData
    {
        public readonly LayerProcLiteChunkKey Key;
        public readonly LayerProcLiteWorldBounds Bounds;
        public readonly TData Data;
        public readonly JobHandle Handle;

        public LayerProcLiteProviderChunk(
            LayerProcLiteChunkKey key,
            LayerProcLiteWorldBounds bounds,
            TData data,
            JobHandle handle)
        {
            Key = key;
            Bounds = bounds;
            Data = data;
            Handle = handle;
        }
    }
}
