using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerInitialConstructionSiteSpawnSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned || HasAnyConstructionSite())
            {
                _spawned = true;
                return;
            }

            var resource = SW.GetResource<Stage1SettlementSeed>();
            var sites = resource.InitialConstructionSites;
            for (var i = 0; i < sites.Length; i++)
                SpawnInitialSite(sites[i]);

            _spawned = true;
        }

        private static void SpawnInitialSite(Stage1ConstructionSiteSeed definition)
        {
            var buildingDefinition = BuildingCatalogData.Get(new BuildingId(definition.BuildingId));
            var siteGid = ServerBuildingSpawns.SpawnConstructionSite(
                new NetworkPeerId(0),
                buildingDefinition,
                definition.Position,
                definition.Rotation);

            if (!siteGid.TryUnpack<ServerWT>(out var site))
                throw new System.InvalidOperationException("Spawned initial construction site could not be unpacked in server world.");

            if (definition.StartReadyToBuild)
            {
                ref var siteState = ref ReplicationMut.Mut<ConstructionSiteState>(site);
                ref var siteResources = ref ReplicationMut.Mut<ConstructionResources>(site);
                siteResources.WoodDelivered = siteResources.WoodRequired;
                siteResources.StoneDelivered = siteResources.StoneRequired;
                siteState.Phase = ConstructionPhase.ReadyToBuild;
            }

            if (definition.InitialBuildWork <= 0f)
                return;

            ref var buildState = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref readonly var buildResources = ref site.Read<ConstructionResources>();
            ref var progress = ref ReplicationMut.Mut<ConstructionProgress>(site);
            ConstructionRules.ApplyBuildWork(
                ref buildState,
                ref progress,
                in buildResources,
                definition.InitialBuildWork,
                progress.BuildWorkRequired);
        }

        private static bool HasAnyConstructionSite()
        {
            foreach (var _ in SW.Query<All<ConstructionSiteTag>>().Entities())
                return true;

            return false;
        }
    }
}
