using StaticMlp.LayerProcLite;
using Unity.Collections;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldSurfaceChunkData : ILayerProcLiteChunkData
    {
        public readonly LayerProcLiteGridLayout Layout;
        public readonly NativeArray<OpenWorldNativeSurfaceSample> Surfaces;

        public OpenWorldSurfaceChunkData(
            LayerProcLiteGridLayout layout,
            NativeArray<OpenWorldNativeSurfaceSample> surfaces)
        {
            Layout = layout;
            Surfaces = surfaces;
        }

        public void Dispose()
        {
            Surfaces.Dispose();
        }
    }
}
