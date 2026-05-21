using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerExtractionOperationSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<FinishedBuildingTag, ExtractionOperationState>>().Entities())
            {
                ref var state = ref entity.Mut<ExtractionOperationState>();
                ExtractionRules.FillBuffer(ref state);
            }
        }
    }
}
