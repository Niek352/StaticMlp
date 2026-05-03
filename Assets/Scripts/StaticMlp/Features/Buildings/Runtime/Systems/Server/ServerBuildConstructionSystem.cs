using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerBuildConstructionSystem : ISystem
    {
        private readonly float _interactionRange;
        private readonly float _maxWorkPerRequest;

        public ServerBuildConstructionSystem(float interactionRange = 4f, float maxWorkPerRequest = 5f)
        {
            _interactionRange = interactionRange;
            _maxWorkPerRequest = maxWorkPerRequest;
        }

        public void Update()
        {
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var evt in inbox.Events)
            {
                if (evt.EventTypeId != GameplayEventTypeIds.BuildConstructionRequest)
                    continue;

                if (!ConstructionEventCodec.TryReadBuild(evt.Payload, out var request))
                    continue;

                if (!TryGetSite(request.Site, out var site))
                    continue;

                if (!ServerConstructionAuthorization.OwnsSite(site, evt.SourcePeer))
                    continue;

                var transform = site.Read<ConstructionTransform>();
                if (!ServerConstructionAuthorization.IsPlayerNear(evt.SourcePeer, transform.Position, _interactionRange))
                    continue;

                ref readonly var resources = ref site.Read<ConstructionResources>();
                if (!resources.IsComplete)
                    continue;

                ref var state = ref site.Mut<ConstructionSiteState>();
                if (state.Phase != ConstructionPhase.ReadyToBuild && state.Phase != ConstructionPhase.BuildingInProgress)
                    continue;

                var work = Mathf.Clamp(request.WorkAmount, 0f, _maxWorkPerRequest);
                if (work <= 0f)
                    continue;

                ref var progress = ref site.Mut<ConstructionProgress>();
                progress.BuildWorkDone = Mathf.Min(progress.BuildWorkRequired, progress.BuildWorkDone + work);
                state.Phase = progress.IsComplete
                    ? ConstructionPhase.Completed
                    : ConstructionPhase.BuildingInProgress;
            }
        }

        private static bool TryGetSite(EntityGID gid, out SW.Entity site)
        {
            if (gid.TryUnpack<ServerWT>(out site)
                && site.Has<ConstructionSiteTag>()
                && site.Has<ConstructionSiteState>()
                && site.Has<ConstructionResources>()
                && site.Has<ConstructionProgress>()
                && site.Has<ConstructionTransform>())
                return true;

            site = default;
            return false;
        }
    }
}
