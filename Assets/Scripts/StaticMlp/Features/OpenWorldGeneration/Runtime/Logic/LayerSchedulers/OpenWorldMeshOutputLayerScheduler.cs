using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldMeshOutputLayerScheduler : ILayerProcLiteLayerScheduler
    {
        public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
        {
            var meshData = context.Providers.GetSingleOverlapping<OpenWorldMeshChunkData>(
                OpenWorldGenerationLayerIds.MeshData,
                0,
                context.Bounds);

            return new LayerProcLiteScheduleResult(
                context.DependencyHandle,
                new OpenWorldMeshOutputChunkData(meshData));
        }
    }
}
