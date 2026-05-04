using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Components.Buildings;
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

            if (!StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(new BuildingId(state.BuildingId), out var definition))
                return;

            ServerBuildingSpawns.SpawnFinishedBuilding(owner, definition, transform);
            NetworkEntityDespawner.DespawnAndDestroy(site);
        }
    }
}
