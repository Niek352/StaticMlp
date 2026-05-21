using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildConstructionProjector
        : IRequestProjector<BuildConstructionRequestEvent, BuildConstructionResultEvent>
    {
        private readonly float _maxWorkPerRequest;

        public BuildConstructionProjector(float maxWorkPerRequest = ConstructionActionProfiles.PlayerBuildMaxWorkPerRequest)
        {
            _maxWorkPerRequest = maxWorkPerRequest;
        }

        public void Project(in BuildConstructionRequestEvent request)
        {
            if (!request.Site.TryUnpack<ClientCoreWT>(out var site)
                || !site.Has<ConstructionSiteTag>()
                || !site.Has<Projected<ConstructionSiteState>>()
                || !site.Has<Projected<ConstructionResources>>()
                || !site.Has<Projected<ConstructionProgress>>())
                return;

            ref var state = ref ClientProjection.Mut<ConstructionSiteState>(site);
            ref var progress = ref ClientProjection.Mut<ConstructionProgress>(site);
            SettlementConstructionRules.ApplyProjectedBuildWork(
                site,
                ref state,
                ref progress,
                request.WorkAmount,
                _maxWorkPerRequest);
        }

        public void OnResolved(in BuildConstructionRequestEvent request, in BuildConstructionResultEvent result)
        {
        }
    }
}
