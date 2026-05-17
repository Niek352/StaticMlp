using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldGenerationLayerIds
    {
        public static readonly LayerProcLiteLayerId Height = new(0);
        public static readonly LayerProcLiteLayerId Surface = new(1);
        public static readonly LayerProcLiteLayerId MeshData = new(2);
        public static readonly LayerProcLiteLayerId VisualMesh = new(3);
        public static readonly LayerProcLiteLayerId PhysicsMesh = new(4);
        public static readonly LayerProcLiteLayerId NavMeshSource = new(5);
        public static readonly LayerProcLiteLayerId Placements = new(6);
    }
}
