using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldMeshOutputChunkData : ILayerProcLiteChunkData
    {
        public readonly OpenWorldMeshChunkData MeshData;

        public OpenWorldMeshOutputChunkData(OpenWorldMeshChunkData meshData)
        {
            MeshData = meshData;
        }

        public void Dispose()
        {
        }
    }
}
