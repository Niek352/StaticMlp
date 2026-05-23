using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Settlement;
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

            var resource = SW.GetResource<SettlementSeed>();
            var heightSampler = SW.GetResource<IHeightSampler>();
            var sites = resource.InitialConstructionSites;
            for (var i = 0; i < sites.Length; i++)
                SpawnInitialSite(sites[i], heightSampler);

            _spawned = true;
        }

        private static void SpawnInitialSite(SettlementConstructionSiteSeed definition, IHeightSampler heightSampler)
        {
            var position = definition.Position;
            position.y = heightSampler.SampleHeight(position.x, position.z);

            var buildingDefinition = BuildingCatalogData.Get(new BuildingId(definition.BuildingId));
            var siteGid = SW.GetResource<BuildingEntityFactory>().SpawnConstructionSite(new ConstructionSiteSpawnSpec(
                new NetworkPeerId(0),
                buildingDefinition,
                new SettlementAnchorId(definition.AnchorId),
                position,
                definition.Rotation,
                definition.StartReadyToBuild,
                definition.InitialBuildWork));

            if (!siteGid.TryUnpack<ServerWT>(out var site))
                throw new System.InvalidOperationException("Spawned initial construction site could not be unpacked in server world.");
        }

        private static bool HasAnyConstructionSite()
        {
            foreach (var _ in SW.Query<All<ConstructionSiteTag>>().Entities())
                return true;

            return false;
        }
    }
}
