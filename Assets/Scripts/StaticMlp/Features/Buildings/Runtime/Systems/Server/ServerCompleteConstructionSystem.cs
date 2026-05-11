using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerCompleteConstructionSystem : ISystem
    {
        private readonly List<EntityGID> _completedSites = new();

        public void Update()
        {
            _completedSites.Clear();

            foreach (var e in SW.Query<All<ConstructionSiteTag, ConstructionSiteState>>().Entities())
            {
                ref readonly var state = ref e.Read<ConstructionSiteState>();
                if (state.Phase == ConstructionPhase.Completed)
                    _completedSites.Add(e.GID);
            }

            for (var i = 0; i < _completedSites.Count; i++)
                Complete(_completedSites[i]);
        }

        private static void Complete(EntityGID gid)
        {
            if (!ConstructionSiteQuery.TryGetConstructionSite(gid, out var site))
                return;

            if (!NetworkEntityOwnership.TryGetOwner(site, out var owner))
                return;

            var state = site.Read<ConstructionSiteState>();
            var transform = site.Read<ConstructionTransform>();
            var definition = StaticMlp.Features.BuildingCatalog.BuildingCatalogData.Get(new BuildingId(state.BuildingId));
            var hasProgression = site.Has<Stage1SettlementProgression>();
            var progression = hasProgression
                ? site.Read<Stage1SettlementProgression>()
                : default;

            if (hasProgression && progression.Stage != Stage1SettlementProgressStage.CampRepaired)
                progression.AdvanceTo(Stage1SettlementProgressStage.CampRepaired);

            var finishedGid = ServerBuildingSpawns.SpawnFinishedBuilding(
                owner,
                definition,
                transform,
                hasProgression
                    ? entity => entity.Set(progression)
                    : null);
            if (!finishedGid.TryUnpack<ServerWT>(out var finishedBuilding))
                throw new System.InvalidOperationException("Spawned finished building could not be unpacked in server world.");

            NetworkEntityDespawner.DespawnAndDestroy(site);
        }
    }
}
