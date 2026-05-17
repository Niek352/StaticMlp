using StaticMlp.LayerProcLite;
using Unity.Collections;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldHeightChunkData : ILayerProcLiteChunkData
    {
        public readonly LayerProcLiteGridLayout Layout;
        public readonly NativeArray<float> PaddedHeights;

        public OpenWorldHeightChunkData(LayerProcLiteGridLayout layout, NativeArray<float> paddedHeights)
        {
            Layout = layout;
            PaddedHeights = paddedHeights;
        }

        public void Dispose()
        {
            PaddedHeights.Dispose();
        }
    }
}
